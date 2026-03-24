using Microsoft.EntityFrameworkCore;
using SpectraLiveApi.Data;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Endpoints;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Services;
using SpectraLiveApi.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TwitchService>();
builder.Services.AddScoped<UnviewsService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnviewsRepository, UnviewsRepository>();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["SpectraLive:FrontendUrl"] ?? "http://localhost:8000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient<TwitchAuthClient>(client =>
    client.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));
builder.Services.AddHttpClient<TwitchApiClient>(client => 
    client.BaseAddress = new Uri("https://api.twitch.tv/helix/"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["SpectraLive:SecretKey"];
        var keyBytes = Encoding.ASCII.GetBytes(secretKey!);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<TwitchSettings>(builder.Configuration.GetSection("Twitch"));
builder.Services.Configure<SpectraLiveSettings>(builder.Configuration.GetSection("SpectraLive"));

var app = builder.Build();

app.UseCors();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapPrefsEndpoints();
app.MapInfoEndpoints();

app.Run("http://localhost:8000");