using NLog;
using NLog.Web;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreApi_BL_App.Services;
using Microsoft.Extensions.Configuration;

var handler = new HttpClientHandler();
// Use caution when accepting any server certificate in production environments
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

var client = new HttpClient(handler);
var builder = WebApplication.CreateBuilder(args);

// Register DatabaseManager as a singleton service with the connection string
builder.Services.AddSingleton<DatabaseManager>(sp =>
    new DatabaseManager(builder.Configuration.GetConnectionString("defaultConnectionbeta")));

// Set up directory for logs
var logDir = Path.Combine("C:", "Logs", "LogManager");
if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}

// Set up NLog for ASP.NET Core
builder.Logging.ClearProviders(); // Remove default logging providers
builder.Host.UseNLog(); // Use NLog as the logging provider

// Register controllers and other services
builder.Services.AddControllers();

// Set up CORS policy for specific origins or a broader policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://qa.vcqru.com/") // Allow any origin
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Enable Swagger for API documentation (only in development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom database service
builder.Services.AddScoped<IDatabaseService, SQL_DB>();

var app = builder.Build();

// Configure Swagger for Development Environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use the CORS policy defined earlier
app.UseCors("AllowAll");

// Enable HTTPS redirection (ensure your app is configured for HTTPS)
app.UseHttpsRedirection();

// Authentication and Authorization Middleware (only if needed)
app.UseAuthentication(); // Enable if authentication is configured
app.UseAuthorization();  // Enable if authorization is configured

// Map controllers to the app (routes)
app.MapControllers();

// Run the application
app.Run();
