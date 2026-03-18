using System.Net;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs.Twitch;
using SpectraLiveApi.Common;
using SpectraLiveApi.Settings;

namespace SpectraLiveApi.Integrations;

public class TwitchApiClient
{
	private readonly HttpClient _httpClient;
	//private readonly IOptions<TwitchSettings> _options;
	private readonly string _clientId;

	public TwitchApiClient(HttpClient httpClient, IOptions<TwitchSettings> options)
	{
		_httpClient = httpClient;
		_clientId = options.Value.ClientId;
	}

	public async Task<Result<TwitchUserData, TwitchUserResponse>> GetUserProfile(string accessToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "users");

		request.Headers.Add("Authorization", $"Bearer {accessToken}");
		request.Headers.Add("Client-Id", _clientId);

		var response = await _httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
            Console.WriteLine($"Erro Twitch: {await response.Content.ReadAsStringAsync()}");

			return new Result<TwitchUserData, TwitchUserResponse> { Error = new TwitchUserResponse(response.StatusCode, response.ReasonPhrase ?? "Erro ao fazer request para a Twitch.") };
		}

		var result = await response.Content.ReadFromJsonAsync<TwitchUserRequest>();

		if (result == null)
			return new Result<TwitchUserData, TwitchUserResponse> { Error = new TwitchUserResponse(HttpStatusCode.BadRequest, "Usuário não encontrado.") };

		var userData = result.Data.First();

		return new Result<TwitchUserData, TwitchUserResponse> { Success = userData };
	}
}
