using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
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

// Configure JSON options so that the API spec works
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SessionTokenScheme";
    options.DefaultChallengeScheme = "SessionTokenScheme";
})
.AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("SessionTokenScheme", null);

// Add services
builder.Services.AddSingleton<PasswordHasher<AppUser>>();
builder.Services.AddSingleton<JwtTokenService>();
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

// Add HTTPS redirection if there's an HTTPS URL
string urls = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey) ?? "";
if (urls.ToLower().Contains("https"))
{
    app.UseHttpsRedirection();
}

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add endpoints for controller actions
app.MapControllers();
#endregion

// Run the app
app.Run();
