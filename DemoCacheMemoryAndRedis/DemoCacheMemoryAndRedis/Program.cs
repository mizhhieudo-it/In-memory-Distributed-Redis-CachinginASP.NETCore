using DemoCacheMemoryAndRedis.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTransient<MovieDBContext>();
builder.Services.AddControllers();
#region config cache with Memory
builder.Services.AddMemoryCache();
#endregion
#region Config cache with Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisHost");
    // options.InstanceName = "LocalhostRedis_";
});
// config service Cache use Sql cache 

#endregion
#region Config Cache With Sql Cache 
builder.Services.AddDistributedSqlServerCache
    (
    options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("CacheDbConnection");
        options.SchemaName = "dbo";
        options.TableName = "CacheStore";
    }

    );

#endregion



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
