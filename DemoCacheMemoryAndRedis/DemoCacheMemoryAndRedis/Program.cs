using DemoCacheMemoryAndRedis.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTransient<MovieDBContext>();
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // config cache in memory
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = "localhost:1000"; // config cache in Redis
});


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
