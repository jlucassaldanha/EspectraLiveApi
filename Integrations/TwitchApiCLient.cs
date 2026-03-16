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

	public async Task<TwitchUserResponse?> GetUserProfile(string accessToken, string clientId)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "users");
		request.Headers.Add("Authorization", $"Bearer {accessToken}");
		request.Headers.Add("Client-Id", clientId);

		var response = await _httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		var result = await response.Content.ReadFromJsonAsync<TwitchUserResponse>();

		return result?.Data.FirstOrDefault();
	}
	
}
