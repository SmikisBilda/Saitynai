using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AccesPointsSaitynai API",
        Version = "v1",
        Description = "OpenAPI specification for the AccesPointsSaitynai service"
    });

    // Include XML comments if available (enable GenerateDocumentationFile in csproj)
    var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseStaticFiles();

app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI();



// Map controllers
app.MapControllers();
app.MapStaticAssets();

app.Run();

