using Microsoft.EntityFrameworkCore;
using vendingmachines.queries.application;
using vendingmachines.queries.consumers;
using vendingmachines.queries.controllers.Dtos;
using vendingmachines.queries.repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MachineMapperProfile));
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("db")));
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddHostedService<MachineCreatedTopicConsumer>();
//builder.Services.AddHostedService<ProductAddedTopicConsumer>();
//builder.Services.AddHostedService<ProductQtyUpdatedTopicConsumer>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
