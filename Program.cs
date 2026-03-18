using Microsoft.EntityFrameworkCore;
using SpectraLiveApi.Data;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Endpoints;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Services;
using SpectraLiveApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddMemoryCache();

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

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.Configure<TwitchSettings>(builder.Configuration.GetSection("Twitch"));
builder.Services.Configure<SpectraLiveSettings>(builder.Configuration.GetSection("SpectraLive"));

var app = builder.Build();

app.UseCors();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapPrefsEndpoints();

app.Run("http://localhost:8000");