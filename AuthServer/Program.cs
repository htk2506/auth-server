using AuthServer.Database;
using AuthServer.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// Build the app
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddOpenApi();
// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention();
});
// Use snake case for JSON request bodies
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
// Add PasswordHasher
builder.Services.AddSingleton<PasswordHasher<User>>();
var app = builder.Build();


// Configure the app
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
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// Run the app
app.Run();
