using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Common;
using SpectraLiveApi.Settings;
using SpectraLiveApi.DTOs.Twitch;

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

	public async Task<Result<TwitchAuthRequest, TwitchAuthResponse>> GetAuthToken(string code, string redirectUri)
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
			var authResult = await response.Content.ReadFromJsonAsync<TwitchAuthRequest>();
			return new Result<TwitchAuthRequest, TwitchAuthResponse> { Success = authResult };
		}
		else
		{
			var authError = await response.Content.ReadFromJsonAsync<TwitchAuthResponse>();
			return new Result<TwitchAuthRequest, TwitchAuthResponse> { Error = authError };
		}
	}

	public async Task<Result<TwitchRefreshTokenRequest, TwitchRefreshTokenResponse>> GetRefreshToken(string refreshToken)
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
			var refreshTokenResult = await response.Content.ReadFromJsonAsync<TwitchRefreshTokenRequest>();
			return new Result<TwitchRefreshTokenRequest, TwitchRefreshTokenResponse> { Success = refreshTokenResult };
		}
		else
		{
			var refreshTokenError = await response.Content.ReadFromJsonAsync<TwitchRefreshTokenResponse>();
			return new Result<TwitchRefreshTokenRequest, TwitchRefreshTokenResponse> { Error = refreshTokenError };
		}
	}
}
