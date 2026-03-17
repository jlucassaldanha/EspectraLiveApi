using SpectraLiveApi.DTOs;
using SpectraLiveApi.Endpoints;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Services;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration["SpectraLive:FrontendUrl"] ?? "http://localhost:8000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();

builder.Services.Configure<TwitchSettings>(builder.Configuration.GetSection("Twitch"));
builder.Services.Configure<SpectraLiveSettings>(builder.Configuration.GetSection("SpectraLive"));

builder.Services.AddHttpClient<TwitchAuthClient>((HttpClient client) =>
{
    client.BaseAddress = new Uri("https://id.twitch.tv/oauth2/");
});

builder.Services.AddHttpClient<TwitchApiClient>((HttpClient client) => 
{
    client.BaseAddress = new Uri("https://api.twitch.tv/helix/");
});

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseCors();

app.MapAuthEndpoints();

app.Run("http://localhost:8000");