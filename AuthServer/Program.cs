using Asp.Versioning;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
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

// Configure problem details
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        IExceptionHandlerPathFeature? exceptionHandler = context.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandler != null)
        {
            // Add info from exceptions
            Exception error = exceptionHandler.Error;
            context.ProblemDetails.Type = exceptionHandler.Error.GetType().Name;
            context.ProblemDetails.Detail = exceptionHandler.Error.Message;
        }
    };
});

// Add API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON options
        options.JsonSerializerOptions.WriteIndented = true;
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
    options.DefaultAuthenticateScheme = "SessionJwtScheme";
    options.DefaultChallengeScheme = "SessionJwtScheme";
})
    .AddScheme<AuthenticationSchemeOptions, SessionJwtAuthenticationHandler>("SessionJwtScheme", null);

// Add Swagger doc generation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Server", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Auth Server", Version = "v2" });

    // Add Bearer token authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Session token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Add health check
builder.Services.AddHealthChecks();

// Add services
builder.Services.AddSingleton<PasswordHasher<AppUser>>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<EmailService>();
#endregion

#region Configure the app
var app = builder.Build();

// Use Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
});

// Use HTTPS redirection if there's an HTTPS URL
string urls = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey) ?? "";
if (urls.ToLower().Contains("https"))
{
    app.UseHttpsRedirection();
}

// Catch exceptions
app.UseExceptionHandler();

// Enable problem details to be returned when error response is otherwise empty
app.UseStatusCodePages();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map the health check endpoint
app.MapHealthChecks("/healthz");

// Map endpoints for controller actions
app.MapControllers();
#endregion

// Run the app
app.Run();
