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

	public async Task<Result<IEnumerable<TwitchUsersIdsData>>> GetUserMods(string accessToken, string twitchId)
	{
		List<TwitchUsersIdsData> currentMods = [];

		string? cursor = null;

		do
		{
			var queryParams = $"broadcaster_id={twitchId}&first=100";

			if (!string.IsNullOrEmpty(cursor))
				queryParams += $"&after={cursor}";
			
			var request = new HttpRequestMessage(HttpMethod.Get, $"moderation/moderators?{queryParams}");

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
					
				return Result<IEnumerable<TwitchUsersIdsData>>.Failure(new Error(errorMessage, response.StatusCode));
			}

			var result = await response.Content.ReadFromJsonAsync<TwitchUsersIdsResponse>();
			
			if (result?.Data == null)
				return Result<IEnumerable<TwitchUsersIdsData>>.Failure(new Error("Usuário não encontrado.", HttpStatusCode.InternalServerError));

			currentMods.AddRange(result.Data);

			cursor = result.Pagination?.Cursor;
		} while (!string.IsNullOrEmpty(cursor));

		return Result<IEnumerable<TwitchUsersIdsData>>.Success(currentMods);
	}

	public async Task<Result<IEnumerable<TwitchUserData>>> GetUsersData(string accessToken, IEnumerable<string> twitchIds)
	{
		var queryParams = $"id={twitchIds.ElementAt(0)}";
		foreach(var id in twitchIds.Skip(1))
		{
			queryParams += $"&id={id}";
		}

		var request = new HttpRequestMessage(HttpMethod.Get, $"users?{queryParams}");

		request.Headers.Add("Authorization", $"Bearer {accessToken}");
		request.Headers.Add("Client-Id", _options.Value.ClientId);

		var response = await _httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
            var errorData = await response.Content.ReadFromJsonAsync<TwitchErrorResponse>();
			var errorMessage = errorData?.Message ?? errorData?.Error ?? response.ReasonPhrase ?? "Erro ao contatar a Twitch";
			
			return Result<IEnumerable<TwitchUserData>>.Failure(new Error(errorMessage, response.StatusCode));
		}

		var result = await response.Content.ReadFromJsonAsync<TwitchUserResponse>();

		if (result == null)
			return Result<IEnumerable<TwitchUserData>>.Failure(new Error("Usuários não encontrados.", HttpStatusCode.InternalServerError));

		return Result<IEnumerable<TwitchUserData>>.Success(result.Data);
	}

	public async Task<Result<IEnumerable<TwitchUsersIdsData>>> GetChatters(string accessToken, string twitchId)
	{
		List<TwitchUsersIdsData> currentChatters = [];

		string? cursor = null;

		do
		{
			var queryParams = $"broadcaster_id={twitchId}&moderator_id={twitchId}&first=1000";

			if (!string.IsNullOrEmpty(cursor))
				queryParams += $"&after={cursor}";
			
			var request = new HttpRequestMessage(HttpMethod.Get, $"chat/chatters?{queryParams}");

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
					
				return Result<IEnumerable<TwitchUsersIdsData>>.Failure(new Error(errorMessage, response.StatusCode));
			}

			var result = await response.Content.ReadFromJsonAsync<TwitchUsersIdsResponse>();
			
			if (result?.Data == null)
				return Result<IEnumerable<TwitchUsersIdsData>>.Failure(new Error("Usuário não encontrado.", HttpStatusCode.InternalServerError));

			currentChatters.AddRange(result.Data);

			cursor = result.Pagination?.Cursor;
		} while (!string.IsNullOrEmpty(cursor));

		return Result<IEnumerable<TwitchUsersIdsData>>.Success(currentChatters);
	}
}
