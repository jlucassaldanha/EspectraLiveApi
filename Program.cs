using SpectraLiveApi.DTOs;
using SpectraLiveApi.Endpoints;
using SpectraLiveApi.Integrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TwitchSettings>(builder.Configuration.GetSection("Twitch"));
builder.Services.Configure<SpectraLiveSettings>(builder.Configuration.GetSection("SpectraLive"));

builder.Services.AddHttpClient<TwitchAuthClient>((HttpClient client) =>
{
    client.BaseAddress = new Uri("https://id.twitch.tv/oauth2");
});

var app = builder.Build();

app.MapAuthEndpoints();

app.Run();