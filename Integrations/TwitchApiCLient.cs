using System.Net;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs.Twitch;
using SpectraLiveApi.Common;
using SpectraLiveApi.Settings;

namespace SpectraLiveApi.Integrations;

public class TwitchApiClient
{
	private readonly HttpClient _httpClient;
	private readonly IOptions<TwitchSettings> _options;

	public TwitchApiClient(HttpClient httpClient, IOptions<TwitchSettings> options)
	{
		_httpClient = httpClient;
		_options = options;
	}

	public async Task<Result<TwitchUserData>> GetUserProfile(string accessToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "users");

		request.Headers.Add("Authorization", $"Bearer {accessToken}");
		request.Headers.Add("Client-Id", _options.Value.ClientId);

		var response = await _httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
            var errorData = await response.Content.ReadFromJsonAsync<TwitchErrorResponse>();
			var errorMessage = errorData?.Message ?? errorData?.Error ?? response.ReasonPhrase ?? "Erro ao contatar a Twitch";
			
			return Result<TwitchUserData>.Failure(new Error(errorMessage, response.StatusCode));
		}

		var result = await response.Content.ReadFromJsonAsync<TwitchUserResponse>();

		if (result == null)
			return Result<TwitchUserData>.Failure(new Error("Usuário não encontrado.", HttpStatusCode.InternalServerError));

		var userData = result.Data.First();

		return Result<TwitchUserData>.Success(userData);
	}
}
