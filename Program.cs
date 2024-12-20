using NLog;
using NLog.Web;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreApi_BL_App.Services;
using Microsoft.Extensions.Configuration;

var handler = new HttpClientHandler();

// Allowing all certificates in case of self-signed certificates (for development only)
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

var client = new HttpClient(handler);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();


// Register DatabaseManager service with the connection string
builder.Services.AddSingleton<DatabaseManager>(sp =>
    new DatabaseManager(builder.Configuration.GetConnectionString("defaultConnectionbeta"))
);

// Set up directory for logs
var logDir = Path.Combine("C:", "Logs", "LogManager");
if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}

// Clear default logging providers and use NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Allow any origin for local testing or wider access
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger setup for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // This will serve the Swagger JSON at /swagger/v1/swagger.json
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); // Explicitly set the path for the Swagger UI to access
        c.RoutePrefix = string.Empty; // Optionally, you can set this to empty to serve Swagger at the root
    });
}

// Use CORS policy
app.UseCors("AllowAll");

// Redirect HTTP requests to HTTPS (if necessary)
app.UseHttpsRedirection();

// Authentication and Authorization (if required)
app.UseAuthentication();
app.UseAuthorization();

// Map controllers (routes for API)
app.MapControllers();

app.Run();
