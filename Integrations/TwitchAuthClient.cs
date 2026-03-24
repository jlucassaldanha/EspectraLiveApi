using Microsoft.Extensions.Options;
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

	public async Task<Result<TwitchAuthResponse>> GetAuthToken(string code, string redirectUri)
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
			return Result<TwitchAuthResponse>.Success(authResult!);
		}
		else
		{
			var authError = await response.Content.ReadFromJsonAsync<TwitchErrorResponse>();	
			var errorMessage = authError?.Message ?? authError?.Error ?? "Erro desconhecido.";

			return Result<TwitchAuthResponse>.Failure(new Error(errorMessage, response.StatusCode));
		}
	}

	public async Task<Result<TwitchRefreshTokenResponse>> GetRefreshToken(string refreshToken)
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
			return Result<TwitchRefreshTokenResponse>.Success(refreshTokenResult!);
		}
		else
		{
			var refreshTokenError = await response.Content.ReadFromJsonAsync<TwitchErrorResponse>();
			var errorMessage = refreshTokenError?.Message ?? refreshTokenError?.Error ?? "Erro desconhecido.";
			
			return Result<TwitchRefreshTokenResponse>.Failure(new Error(errorMessage, response.StatusCode));
		}
	}
}
