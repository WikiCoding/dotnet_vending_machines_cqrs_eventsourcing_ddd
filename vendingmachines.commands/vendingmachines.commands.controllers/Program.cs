using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;
using vendingmachines.commands.application;
using vendingmachines.commands.cmds;
using vendingmachines.commands.controllers.ExceptionHandler;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.eventsourcinghandler;
using vendingmachines.commands.eventstore;
using vendingmachines.commands.persistence.MongoDbConfig;
using vendingmachines.commands.persistence.Repository;
using vendingmachines.commands.producer;

var builder = WebApplication.CreateBuilder(args);

var assemblies = new[]
{
    typeof(Program).Assembly,
    typeof(CreateMachine).Assembly,
    typeof(CreateMachineCommand).Assembly,
    typeof(MachineCreatedEvent).Assembly
};

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoSessionFactory, MongoSessionFactory>();

builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<EventSourcingHandler>();
builder.Services.AddScoped<EventStore>();
builder.Services.AddScoped<CheckMachineStatus>();
builder.Services.AddExceptionHandler<ExHandler>();
var kafkaConfig = builder.Configuration.GetSection("Kafka").Get<KafkaConfig>() ?? new KafkaConfig();
builder.Services.AddSingleton(kafkaConfig);
builder.Services.AddScoped<KafkaProducer>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<SnapshotsRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exceptionHandler = context.RequestServices.GetRequiredService<ExHandler>();
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
        {
            await exceptionHandler.TryHandleAsync(context, exception, context.RequestAborted);
        }
    }
});

app.UseAuthorization();

app.MapControllers();

app.Run();
