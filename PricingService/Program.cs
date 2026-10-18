using GlobalExceptionHandler.WebApi;
using Newtonsoft.Json;
using PricingService.Configuration;
using PricingService.DataAccess.Marten;
using PricingService.Infrastructure;
using PricingService.Init;
using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(Program).Assembly));
builder.Services.AddLoggingBehavior();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
builder.Services.AddMarten(connectionString);
builder.Services.AddPricingDemoInitializer();
builder.Services.AddEurekaDiscoveryClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseGlobalExceptionHandler(cfg => cfg.MapExceptions());

await app.UseInitializer();

app.MapControllers();

app.Run();

public partial class Program
{
}
