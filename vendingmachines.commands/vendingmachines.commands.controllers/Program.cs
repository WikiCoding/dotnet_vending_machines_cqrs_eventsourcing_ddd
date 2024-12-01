using Microsoft.AspNetCore.Diagnostics;
using vendingmachines.commands.app;
using vendingmachines.commands.contracts;
using vendingmachines.commands.controllers.ExceptionHandler;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.eventsourcinghandler;
using vendingmachines.commands.eventstore;
using vendingmachines.commands.persistence.MongoDbConfig;
using vendingmachines.commands.persistence.Repository;

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
builder.Services.AddScoped<MongoConfig>();
builder.Services.AddScoped<IEventsRepository, EventsRepository>();
builder.Services.AddScoped<EventSourcingHandler>();
builder.Services.AddScoped<EventStore>();
builder.Services.AddScoped<CheckMachineStatus>();
builder.Services.AddExceptionHandler<ExHandler>();

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
