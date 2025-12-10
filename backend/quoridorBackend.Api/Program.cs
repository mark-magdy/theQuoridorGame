using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuoridorBackend.Api.Middleware;
using QuoridorBackend.BLL.Services;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Data;
using QuoridorBackend.DAL.Repositories;
using QuoridorBackend.DAL.Repositories.Interfaces;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Database
builder.Services.AddDbContext<QuoridorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
Console.WriteLine( "DB connection STRING: " + builder.Configuration.GetConnectionString("DefaultConnection"));
// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "QuoridorApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "QuoridorClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});




// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameValidationService, GameValidationService>();
builder.Services.AddScoped<IGameService, GameService>();

// API
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var app = builder.Build();

app.UseGlobalExceptionHandler();  // MUST BE FIRST
app.UseRequestLogging();           // SECOND

// auto migrate database`
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuoridorDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("quoridor")
            .WithTheme(ScalarTheme.DeepSpace) // Options: DeepSpace, Kepler, Mars, Moon, Solar, etc.
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient); // Set default client snippet
    });
}

app.UseCors("AllowAll");



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
