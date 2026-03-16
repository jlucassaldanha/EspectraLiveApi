using System.Net.Http.Json;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Models;

namespace SpectraLiveApi.Integrations;

public class TwitchAuthClient
{
	private readonly HttpClient _httpClient;

	public TwitchAuthClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<TwitchAuthResponse?> GetAuthToken(string code, string clientId, string clientSecret, string redirectUri)
	{

		var data = new Dictionary<string, string>
		{ 
			{"client_id", clientId}, 
			{"client_secret", clientSecret},
			{"code", code},
			{"grant_type", "authorization_code"},
			{"redirect_uri", redirectUri}
		};

		var content = new FormUrlEncodedContent(data);

		var response = await _httpClient.PostAsync("/token", content);

		response.EnsureSuccessStatusCode();

		var authResult = await response.Content.ReadFromJsonAsync<TwitchAuthResponse>();

		if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
		{
			throw new Exception("Falha ao obter token de autorização");
		}

		return authResult;
	}

	public async Task<TwitchRefreshTokenResponse?> GetRefreshToken(string refreshToken, string clientId, string clientSecret)
	{
		var data = new Dictionary<string, string>
		{ 
			{"client_id", clientId}, 
			{"client_secret", clientSecret},
			{"refresh_token", refreshToken},
			{"grant_type", "refresh_token"}
		};

		var content = new FormUrlEncodedContent(data);

		var response = await _httpClient.PostAsync("/token", content);

		response.EnsureSuccessStatusCode();

		var refreshTokenResult = await response.Content.ReadFromJsonAsync<TwitchRefreshTokenResponse>();

		return refreshTokenResult;
	}
}
