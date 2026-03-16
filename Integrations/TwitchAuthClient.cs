using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;

namespace SpectraLiveApi.Integrations;

public class TwitchAuthClient
{
	private readonly HttpClient _httpClient;
	private readonly IOptions<TwitchSettings> _options;

	public TwitchAuthClient(HttpClient httpClient, IOptions<TwitchSettings> options)
	{
		_httpClient = httpClient;
		_options = options;
	}

	public async Task<Result<TwitchAuthResponse, TwitchAuthError>> GetAuthToken(string code, string redirectUri)
	{

		var data = new Dictionary<string, string>
		{ 
			{"client_id", _options.Value.ClientId}, 
			{"client_secret", _options.Value.ClientSecret},
			{"code", code},
			{"grant_type", "authorization_code"},
			{"redirect_uri", redirectUri}
		};

		var content = new FormUrlEncodedContent(data);

		var response = await _httpClient.PostAsync("token", content);

		if (response.IsSuccessStatusCode)
		{
			var authResult = await response.Content.ReadFromJsonAsync<TwitchAuthResponse>();
			return new Result<TwitchAuthResponse, TwitchAuthError> { Success = authResult };
		}
		else
		{
			var authError = await response.Content.ReadFromJsonAsync<TwitchAuthError>();
			return new Result<TwitchAuthResponse, TwitchAuthError> { Error = authError };
		}
	}

	public async Task<Result<TwitchRefreshTokenResponse, TwitchRefreshTokenError>> GetRefreshToken(string refreshToken)
	{
		var data = new Dictionary<string, string>
		{ 
			{"client_id", _options.Value.ClientId}, 
			{"client_secret", _options.Value.ClientSecret},
			{"refresh_token", refreshToken},
			{"grant_type", "refresh_token"}
		};

		var content = new FormUrlEncodedContent(data);

		var response = await _httpClient.PostAsync("token", content);

		if (response.IsSuccessStatusCode)
		{
			var refreshTokenResult = await response.Content.ReadFromJsonAsync<TwitchRefreshTokenResponse>();
			return new Result<TwitchRefreshTokenResponse, TwitchRefreshTokenError> { Success = refreshTokenResult };
		}
		else
		{
			var refreshTokenError = await response.Content.ReadFromJsonAsync<TwitchRefreshTokenError>();
			return new Result<TwitchRefreshTokenResponse, TwitchRefreshTokenError> { Error = refreshTokenError };
		}

		// Ainda falta a parte que verifica se tem o usuario na memoria e atualiza
	}
}
