using Asp.Versioning;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Middlewares;
using AuthServer.Services;
using Destructurama;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

// Temporary logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    #region Configure the builder
    var builder = WebApplication.CreateBuilder(args);

    string serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? throw new InvalidOperationException("Missing ServiceName.");
    string serviceVersion = builder.Configuration.GetValue<string>("ServiceVersion") ?? throw new InvalidOperationException("Missing ServiceVersion.");

    // Add Serilog
    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .Enrich.WithProperty("ServiceName", serviceName)
        .Enrich.WithProperty("ServiceVersion", serviceVersion)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Destructure.UsingAttributes()
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    // Configure database
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
    });

    // Add exception handlelr
    builder.Services.AddExceptionHandler<ExceptionLoggingHandler>();

    // Configure problem details
    builder.Services.AddProblemDetails();

    // Add routing
    builder.Services.AddRouting(options => options.LowercaseUrls = true);

    // Add CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Add authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "SessionJwtScheme";
        options.DefaultChallengeScheme = "SessionJwtScheme";
    })
        .AddScheme<AuthenticationSchemeOptions, SessionJwtAuthenticationHandler>("SessionJwtScheme", null);

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

    // Add Swagger doc generation
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Server", Version = "v1" });
        options.SwaggerDoc("v2", new OpenApiInfo { Title = "Auth Server", Version = "v2" });

        // Define Bearer token authentication schema
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Session token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });

        // Use Bearer schema
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
        });
    });

    // Add health check
    builder.Services.AddHealthChecks();

    // Add OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion))
        .WithTracing(tracing => tracing
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddMeter(serviceName)
            .AddConsoleExporter());

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

    // Add services
    builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
    builder.Services.AddSingleton<JwtService>();
    builder.Services.AddSingleton<TokenService>();
    builder.Services.AddScoped<EmailService>();
    #endregion

    #region Configure the app
    var app = builder.Build();

    // Add Trace-ID headers
    app.UseMiddleware<TraceIdMiddleware>();

    // Streamline request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            // Add user ID to logs
            string userId = httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" : "";
            diagnosticContext.Set("UserId", userId);
        };
    });

    // Catch exceptions
    app.UseExceptionHandler();

    // Use HTTPS redirection if there's an HTTPS URL
    string urls = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey) ?? "";
    if (urls.ToLower().Contains("https"))
    {
        app.UseHttpsRedirection();
    }

    // Enable problem details to be returned when error response is otherwise empty
    app.UseStatusCodePages();

    // Route to correct endpoint
    app.UseRouting();

    // Use CORS policy
    app.UseCors("AllowAllOrigins");

    // Use authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Add User ID to log context
    app.UseMiddleware<UserLogContextMiddleware>();

    // Use Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });

    // Map the health check endpoint
    app.MapHealthChecks("/healthz");

    // Map endpoints for controller actions
    app.MapControllers();
    #endregion

    // Run the app
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}