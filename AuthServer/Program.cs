using AuthServer.Database;
using AuthServer.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

# region Configure the builder
var builder = WebApplication.CreateBuilder(args);

// Add API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON options
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add routing
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add OpenAPI service  
builder.Services.AddOpenApi();

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
});

// Add PasswordHasher
builder.Services.AddSingleton<PasswordHasher<User>>();
#endregion

#region Configure the app
var app = builder.Build();

// For development environment
if (app.Environment.IsDevelopment())
{
    // Generate Open API spec
    app.MapOpenApi();
    // Add Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

// Send HTTP requests to HTTPS
app.UseHttpsRedirection();

// Add authorization
app.UseAuthorization();

// Add endpoints for controller actions
app.MapControllers();
#endregion

// Run the app
app.Run();
