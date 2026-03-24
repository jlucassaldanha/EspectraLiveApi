using System.Net;
using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;

namespace SpectraLiveApi.Services;

public class TwitchService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly AuthService _authService;
	private readonly IUserRepository _userRepository;

	public TwitchService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi, IUserRepository userRepository, AuthService authService)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_authService = authService;
		_userRepository = userRepository;
	}

	public async Task<Result<TwitchUsersIds>> GetTwitchUserModeratorsIds(string userId, string twitchId)
	{
		var authResponse = await _authService.GetTwitchUserAuthData(userId);

		if (authResponse.Error != null)
			return Result<TwitchUsersIds>.Failure(new Error(authResponse.Error.Message, authResponse.Error.ErrorCode));

		if (authResponse.Data == null)
			return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao buscar dados de autenticação da Twitch", HttpStatusCode.InternalServerError));

		var accessToken = authResponse.Data.AccessToken;
		var refreshToken = authResponse.Data.RefreshToken;

		var response = await _twitchApi.GetUserMods(accessToken, twitchId);

		if (response.Error != null && response.Error.ErrorCode != HttpStatusCode.Unauthorized)
			return Result<TwitchUsersIds>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));

		if (response.Error?.ErrorCode == HttpStatusCode.Unauthorized)
		{
			var newAccessToken = await _authService.RefreshTwitchToken(twitchId, refreshToken);

			if (newAccessToken.Error != null)
				return Result<TwitchUsersIds>.Failure(new Error(newAccessToken.Error.Message, newAccessToken.Error.ErrorCode));
			if (newAccessToken.Data == null)
				return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao usar refresh token da Twitch", HttpStatusCode.InternalServerError));

			response = await _twitchApi.GetUserMods(newAccessToken.Data, twitchId);

			if (response.Error != null )
				return Result<TwitchUsersIds>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));
		}

		if (response.Data == null)
			return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao buscar mods na Twitch", HttpStatusCode.InternalServerError));

		var modsIds = response.Data.Select(m => m.UserId).ToList();

		return Result<TwitchUsersIds>.Success(new TwitchUsersIds(modsIds));
	}

	public async Task<Result<TwitchUsersIds>> GetTwitchUserChattersIds(string userId, string twitchId)
	{
		var authResponse = await _authService.GetTwitchUserAuthData(userId);

		if (authResponse.Error != null)
			return Result<TwitchUsersIds>.Failure(new Error(authResponse.Error.Message, authResponse.Error.ErrorCode));

		if (authResponse.Data == null)
			return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao buscar dados de autenticação da Twitch", HttpStatusCode.InternalServerError));

		var accessToken = authResponse.Data.AccessToken;
		var refreshToken = authResponse.Data.RefreshToken;

		var response = await _twitchApi.GetChatters(accessToken, twitchId);

		if (response.Error != null && response.Error.ErrorCode != HttpStatusCode.Unauthorized)
			return Result<TwitchUsersIds>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));

		if (response.Error?.ErrorCode == HttpStatusCode.Unauthorized)
		{
			var newAccessToken = await _authService.RefreshTwitchToken(twitchId, refreshToken);

			if (newAccessToken.Error != null)
				return Result<TwitchUsersIds>.Failure(new Error(newAccessToken.Error.Message, newAccessToken.Error.ErrorCode));
			if (newAccessToken.Data == null)
				return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao usar refresh token da Twitch", HttpStatusCode.InternalServerError));

			response = await _twitchApi.GetChatters(newAccessToken.Data, twitchId);

			if (response.Error != null )
				return Result<TwitchUsersIds>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));
		}

		if (response.Data == null)
			return Result<TwitchUsersIds>.Failure(new Error("Erro inesperado ao buscar chatters na Twitch", HttpStatusCode.InternalServerError));

		var chattersIds = response.Data.Select(c => c.UserId).ToList();

		return Result<TwitchUsersIds>.Success(new TwitchUsersIds(chattersIds));
	}

	public async Task<Result<IEnumerable<TwitchUserProfile>>> GetTwitchUsersData(string userId, string twitchUserId, List<string> twitchIds)
	{
		var authResponse = await _authService.GetTwitchUserAuthData(userId);

		if (authResponse.Error != null)
			return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error(authResponse.Error.Message, authResponse.Error.ErrorCode));

		if (authResponse.Data == null)
			return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error("Erro inesperado ao buscar dados de autenticação da Twitch", HttpStatusCode.InternalServerError));

		var accessToken = authResponse.Data.AccessToken;
		var refreshToken = authResponse.Data.RefreshToken;

		var response = await _twitchApi.GetUsersData(accessToken, twitchIds);

		if (response.Error != null && response.Error.ErrorCode != HttpStatusCode.Unauthorized)
			return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));

		if (response.Error?.ErrorCode == HttpStatusCode.Unauthorized)
		{
			var newAccessToken = await _authService.RefreshTwitchToken(twitchUserId, refreshToken);

			if (newAccessToken.Error != null)
				return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error(newAccessToken.Error.Message, newAccessToken.Error.ErrorCode));
			if (newAccessToken.Data == null)
				return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error("Erro inesperado ao usar refresh token da Twitch", HttpStatusCode.InternalServerError));

			response = await _twitchApi.GetUsersData(newAccessToken.Data, twitchIds);

			if (response.Error != null )
				return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));
		}

		if (response.Data == null)
			return Result<IEnumerable<TwitchUserProfile>>.Failure(new Error("Erro inesperado ao buscar mods na Twitch", HttpStatusCode.InternalServerError));

		var usersList = response.Data.Select(u => new TwitchUserProfile(u.Id, u.DisplayName, u.ProfileImgUrl)).ToList();

		return Result<IEnumerable<TwitchUserProfile>>.Success(usersList);
	}
}