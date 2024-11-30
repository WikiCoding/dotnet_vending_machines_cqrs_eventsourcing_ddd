using vendingmachines.commands.app;
using vendingmachines.commands.contracts;
using vendingmachines.commands.domain.DomainEvents;
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

app.UseAuthorization();

app.MapControllers();

app.Run();
