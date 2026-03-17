using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Models;

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

	public async Task<Result<TwitchUserData, TwitchUserError>> GetUserProfile(string accessToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "users");
		request.Headers.Add("Authorization", $"Bearer {accessToken}");
		request.Headers.Add("Client-Id", _options.Value.ClientId);

		var response = await _httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
			return new Result<TwitchUserData, TwitchUserError> { Error = new TwitchUserError(response.StatusCode, "Erro ao fazer request para a Twitch.") };

		var result = await response.Content.ReadFromJsonAsync<TwitchUserResponse>();

		if (result == null)
			return new Result<TwitchUserData, TwitchUserError> { Error = new TwitchUserError(HttpStatusCode.BadRequest, "Usuário não encontrado.") };

		var userData = result.Data.First();

		return new Result<TwitchUserData, TwitchUserError> { Success = userData };
	}
}
