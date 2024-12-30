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
=======
// Use caution when accepting any server certificate in production environments

handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

var client = new HttpClient(handler);
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();


// Register DatabaseManager service with the connection string
builder.Services.AddSingleton<DatabaseManager>(sp =>
    new DatabaseManager(builder.Configuration.GetConnectionString("defaultConnectionbeta"))
);
=======
// Register DatabaseManager as a singleton service with the connection string
builder.Services.AddSingleton<DatabaseManager>(sp =>
    new DatabaseManager(builder.Configuration.GetConnectionString("defaultConnectionbeta")));


// Set up directory for logs
var logDir = Path.Combine("C:", "Logs", "LogManager");
if (!Directory.Exists(logDir))
{
    Directory.CreateDirectory(logDir);
}

// Clear default logging providers and use NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();
=======
// Set up NLog for ASP.NET Core
builder.Logging.ClearProviders(); // Remove default logging providers
builder.Host.UseNLog(); // Use NLog as the logging provider


// Register controllers and other services
builder.Services.AddControllers();


// Configure CORS
=======
// Set up CORS policy for specific origins or a broader policy

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

=======
// Enable Swagger for API documentation (only in development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom database service
builder.Services.AddScoped<IDatabaseService, SQL_DB>();


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

=======
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
