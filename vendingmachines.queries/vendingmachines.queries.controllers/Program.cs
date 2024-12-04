using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using vendingmachines.queries.application;
using vendingmachines.queries.consumers;
using vendingmachines.queries.controllers.Dtos;
using vendingmachines.queries.controllers.ExceptionHandler;
using vendingmachines.queries.repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MachineMapperProfile));
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("db")));
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddHostedService<MachineCreatedTopicConsumer>();
builder.Services.AddHostedService<ProductAddedTopicConsumer>();
builder.Services.AddHostedService<ProductQtyUpdatedTopicConsumer>();

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
    
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<AppDbContext>();
    
    db!.Database.EnsureDeleted();
    db.Database.Migrate();
    db.Database.EnsureCreated();
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
