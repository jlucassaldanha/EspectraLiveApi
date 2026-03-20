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

	public async Task<Result<IEnumerable<TwitchModsData>>> GetUserMods(string accessToken, string twitchId)
	{
		List<TwitchModsData> currentMods = [];

		string? cursor = null;

		do
		{
			var queryParams = $"broadcaster_id={twitchId}&first=100";

			if (!string.IsNullOrEmpty(cursor))
				queryParams += $"&after={cursor}";
			
			var request = new HttpRequestMessage(HttpMethod.Get, $"moderators?{queryParams}");

			request.Headers.Add("Authorization", $"Bearer {accessToken}");
			request.Headers.Add("Client-Id", _options.Value.ClientId);

			var response = await _httpClient.SendAsync(request);

			if (!response.IsSuccessStatusCode)
			{
				var errorData = await response.Content.ReadFromJsonAsync<TwitchErrorResponse>();
				
				string errorMessage = "Erro ao contatar a Twitch";
				
				if (!string.IsNullOrWhiteSpace(errorData?.Message))
					errorMessage = errorData.Message;
				else if (!string.IsNullOrWhiteSpace(errorData?.Error))
					errorMessage = errorData.Error;
				else if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
					errorMessage = response.ReasonPhrase;
					
				return Result<IEnumerable<TwitchModsData>>.Failure(new Error(errorMessage, response.StatusCode));
			}

			var result = await response.Content.ReadFromJsonAsync<TwitchModsResponse>();

			if (result?.Data == null)
				return Result<IEnumerable<TwitchModsData>>.Failure(new Error("Usuário não encontrado.", HttpStatusCode.InternalServerError));

			currentMods.AddRange(result.Data);

			cursor = result.Pagination?.Cursor;
		} while (!string.IsNullOrEmpty(cursor));

		Console.WriteLine($"DADOS: {currentMods}");

		return Result<IEnumerable<TwitchModsData>>.Success(currentMods);
	}

	/*public async Task<Result<TwitchUserData>> GetUsersData(string accessToken)
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
	}*/
}
