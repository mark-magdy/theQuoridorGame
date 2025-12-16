using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuoridorBackend.Api.Middleware;
using QuoridorBackend.Api.Hubs;
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
    
    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll",
//         builder =>
//         {
//             builder.WithOrigins("http://localhost:3000")
//                    .AllowAnyMethod()
//                    .AllowAnyHeader()
//                     .AllowCredentials();
//         });
// });


builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOnly", policy =>
    {
        policy.WithOrigins( "https://quoridor-frontend.proudrock-dd67c3c3.uaenorth.azurecontainerapps.io" ,
                           "http://localhost:3000",
                           "https://quoridorfrontend.proudrock-dd67c3c3.uaenorth.azurecontainerapps.io",
                           "https://www.quoridor.markmagdy.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => 
    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection));

// Services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameValidationService, GameValidationService>();
builder.Services.AddScoped<IBotEngine, BotEngine>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameRoomService, GameRoomService>();

// SignalR
builder.Services.AddSignalR();

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

// app.UseCors("AllowAll");
app.UseCors("FrontendOnly");



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.Run();
