using SpectraLiveApi.Endpoints;
using SpectraLiveApi.Integrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<TwitchAuthClient>((HttpClient client) =>
{
    client.BaseAddress = new Uri("https://id.twitch.tv/oauth2");
    client.DefaultRequestHeaders.Add("Client_id", "");
});

var app = builder.Build();

app.MapAuthEndpoints();

app.Run();