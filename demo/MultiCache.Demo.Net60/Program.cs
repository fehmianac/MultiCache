using MultiCache.Demo.Net60.Handler;
using MultiCache.StackExchangeRedis.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedisMultiCacheServices(options =>
{
    options.Configuration = "127.0.0.1:6379";
    options.InstanceName = "multi-cache-demo";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", Demo.Handler);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();