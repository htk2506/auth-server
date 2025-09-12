using Microsoft.EntityFrameworkCore;
using AuthServer.Database;
using Microsoft.AspNetCore.Identity;
using AuthServer.Database.Models;

// Build the app
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});
builder.Services.AddSingleton<PasswordHasher<User>>();
var app = builder.Build();

// Configure the app
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Run the app
app.Run();
