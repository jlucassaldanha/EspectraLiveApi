using Microsoft.Extensions.Options;
using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;
using System.Net;

namespace SpectraLiveApi.Services;

public class UserService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly AuthService _authService;
	private readonly IUserRepository _userRepository;

	public UserService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi, IUserRepository userRepository, AuthService authService)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_authService = authService;
		_userRepository = userRepository;
	}

	public async Task<Result<UserProfile>> GetUserProfile(string userId)
	{
		User? userProfileFromDb = await _userRepository.GetProfileByUserIdAsync(Guid.Parse(userId));

		if (userProfileFromDb == null)
			return Result<UserProfile>.Failure(new Error("Usuário não encontrado no banco de dados.", HttpStatusCode.NotFound));

		var userProfile = new UserProfile(
			userProfileFromDb.TwitchId,
			userProfileFromDb.DisplayName,
			userProfileFromDb.ProfileImgUrl,
			userProfileFromDb.Id
		);

		return Result<UserProfile>.Success(userProfile);
	}

	public async Task<Result<TwitchUsersIds>> GetTwitchUserMods(string accessToken, string refreshToken, string twitchId)
	{
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

	public async Task<Result<TwitchUsersIds>> GetTwitchUserChatters(string accessToken, string refreshToken, string twitchId)
	{
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

	public async Task<Result<IEnumerable<TwitchUserProfile>>> GetTwitchUsersData(string accessToken, string refreshToken, string twitchUserId, List<string> twitchIds)
	{
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