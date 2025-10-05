using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);
var baseConn = builder.Configuration.GetConnectionString("DefaultConnection");
var pwdOrPath = Environment.GetEnvironmentVariable("DB_PASSWORD");
string password = pwdOrPath;
if (!string.IsNullOrEmpty(pwdOrPath) && File.Exists(pwdOrPath))
{
    password = File.ReadAllText(pwdOrPath).Trim();
}
var csb = new NpgsqlConnectionStringBuilder(baseConn) { Password = password };
var finalConn = csb.ConnectionString;

builder.Services.AddDbContext<SaitynaiContext>(opt => 
    opt.UseNpgsql(finalConn)
    .UseSnakeCaseNamingConvention());
    
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseStaticFiles();

app.MapOpenApi();
app.UseSwagger();
// Option A: Use Swagger UI to read the static YAML
app.UseSwaggerUI(c =>
{
    // Point Swagger UI to the static YAML file
    c.SwaggerEndpoint("/swagger/api-spec.yaml", "Saitynai API");
    // Optional: make this UI the default route
    c.RoutePrefix = "swagger";
});



// Map controllers
app.MapControllers();
app.MapStaticAssets();

app.Run();

