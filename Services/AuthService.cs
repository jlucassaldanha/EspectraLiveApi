using Microsoft.Extensions.Options;
using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;
using System.Net;

namespace SpectraLiveApi.Services;

public class AuthService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly string _apiUrl;
	private readonly IUserRepository _userRepository;

	public AuthService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi, IOptions<SpectraLiveSettings> options, IUserRepository userRepository)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_apiUrl = options.Value.ApiUrl;
		_userRepository = userRepository;
	}

	public async Task<Result<TwitchAuthData>> GetTwitchAuthDataWithCode(string code)
	{
		var redirectUri = $"{_apiUrl}/auth/callback";

		var authData = await _twitchAuth.GetAuthToken(code, redirectUri);

		if (authData.Error != null)
			return Result<TwitchAuthData>.Failure(new Error(authData.Error.Message, authData.Error.ErrorCode));
		if (authData.Data == null)
			return Result<TwitchAuthData>.Failure(new Error("Resposta inesperada da Twitch", HttpStatusCode.InternalServerError)); 
		
		var twitchAuthData = new TwitchAuthData(
			authData.Data.AccessToken,
			authData.Data.RefreshToken,
			authData.Data.ExpiresIn
		);
		
		return Result<TwitchAuthData>.Success(twitchAuthData);
	}

	public async Task<Result<UserData>> UpsertUserWithTwitchAuthData(TwitchAuthData twitchAuthData)
	{
		string accessToken = twitchAuthData.AccessToken;

		var response = await _twitchApi.GetUserProfile(accessToken);

		if (response.Error != null)
			return Result<UserData>.Failure(new Error(response.Error.Message, response.Error.ErrorCode));
		if (response.Data == null)
			return Result<UserData>.Failure(new Error("Erro inesperado ao buscar usuário na Twitch", HttpStatusCode.InternalServerError));
		
		User freshUser;
		var userDataFromTwitchResponse = response.Data;
		var userDataFromDb = await _userRepository.GetProfileByTwitchIdAsync(userDataFromTwitchResponse.Id);
		
		if (userDataFromDb == null)
		{
			freshUser = new User(
				twitchAuthData.AccessToken, 
				twitchAuthData.RefreshToken,
				twitchAuthData.ExpiresIn,
				userDataFromTwitchResponse.Id,
				userDataFromTwitchResponse.DisplayName,
				userDataFromTwitchResponse.ProfileImgUrl
			);

			await _userRepository.AddAsync(freshUser);
		}
		else
		{
			userDataFromDb.AccessToken = twitchAuthData.AccessToken;
			userDataFromDb.RefreshToken = twitchAuthData.RefreshToken;
			userDataFromDb.ExpiresIn = twitchAuthData.ExpiresIn;
			userDataFromDb.DisplayName = userDataFromTwitchResponse.DisplayName;
			userDataFromDb.ProfileImgUrl = userDataFromTwitchResponse.ProfileImgUrl;

			freshUser = userDataFromDb;

			await _userRepository.UpdateAsync(freshUser);
		}

		var newUserData = new UserData(
			freshUser.AccessToken, 
			freshUser.RefreshToken,
			freshUser.ExpiresIn,
			freshUser.TwitchId,
			freshUser.DisplayName,
			freshUser.ProfileImgUrl,
			freshUser.Id
		);

		return Result<UserData>.Success(newUserData);
	}

	public async Task<Result<TwitchAuthData>> GetTwitchUserAuthData(string userId)
	{
		User? userProfileFromDb = await _userRepository.GetProfileByUserIdAsync(Guid.Parse(userId));

		if (userProfileFromDb == null)
			return Result<TwitchAuthData>.Failure(new Error("Usuário não encontrado no banco de dados.", HttpStatusCode.NotFound));

		var twitchAuthData = new TwitchAuthData(
			userProfileFromDb.AccessToken,
			userProfileFromDb.RefreshToken,
			userProfileFromDb.ExpiresIn
		);

		return Result<TwitchAuthData>.Success(twitchAuthData);
	}

	public async Task<Result<string>> RefreshTwitchToken(string twitchId, string refreshToken)
	{
		var refreshResponse = await _twitchAuth.GetRefreshToken(refreshToken);
				
		if (refreshResponse.Error != null)
			return Result<string>.Failure(new Error(refreshResponse.Error.Message, refreshResponse.Error.ErrorCode));
		if (refreshResponse.Data == null)
			return Result<string>.Failure(new Error("Erro inesperado ao tentar usar Refresh Token da Twitch", HttpStatusCode.InternalServerError));

		var userFromDb = await _userRepository.GetProfileByTwitchIdAsync(twitchId);
		
		if (userFromDb == null)
			return Result<string>.Failure(new Error("Usuário não encontrado no banco de dados.", HttpStatusCode.NotFound));

		userFromDb.AccessToken = refreshResponse.Data.AccessToken;
		userFromDb.RefreshToken = refreshResponse.Data.RefreshToken;
		userFromDb.ExpiresIn = refreshResponse.Data.ExpiresIn;
		
		await _userRepository.UpdateAsync(userFromDb);

		return Result<string>.Success(userFromDb.AccessToken);
	}
}