using Asp.Versioning;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

# region Configure the builder
var builder = WebApplication.CreateBuilder(args);

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
});

// Add API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON options
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure API endpoint versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Add routing
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SessionTokenScheme";
    options.DefaultChallengeScheme = "SessionTokenScheme";
})
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("SessionTokenScheme", null);

// Add Swagger doc generation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Server", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Auth Server", Version = "v2" });
});

// Add services
builder.Services.AddSingleton<PasswordHasher<AppUser>>();
builder.Services.AddSingleton<JwtTokenService>();
#endregion

#region Configure the app
var app = builder.Build();

// For development environment
if (app.Environment.IsDevelopment())
{
    // Add Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
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
