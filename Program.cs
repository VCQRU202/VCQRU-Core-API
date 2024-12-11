using NLog;
using NLog.Web;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreApi_BL_App.Services;


var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

var client = new HttpClient(handler);
var builder = WebApplication.CreateBuilder(args);

// Set up directory for logs
var logDir = Path.Combine("C:", "Logs", "LogManager");
if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}

// Set up NLog for ASP.NET Core
builder.Logging.ClearProviders();
builder.Host.UseNLog(); // Use NLog as the logging provider

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://qa.vcqru.com/") // Allow any origin
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDatabaseService, SQL_DB>();

var app = builder.Build();

// Configure Swagger for Development Environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Use CORS policy
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Authentication and Authorization Middleware (only if needed)
app.UseAuthentication(); // Only if authentication is configured
app.UseAuthorization();  // Only if authorization is configured

// Map controllers to the app
app.MapControllers();

// Run the app
app.Run();
