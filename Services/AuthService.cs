using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Models;
using SpectraLiveApi.Repositories;

namespace SpectraLiveApi.Services;

public class AuthService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly string _apiUrl;
	private readonly IMemoryCache _cache;
	private readonly IUserRepository _userRepository;

	public AuthService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi, IMemoryCache cache, IOptions<SpectraLiveSettings> options, IUserRepository userRepository)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_apiUrl = options.Value.ApiUrl;
		_cache = cache;
		_userRepository = userRepository;
	}

	public async Task<Result<TempSessionToken, TempSessionError>> GetSessionWithTwitchCode(string code)
	{
		var redirectUri = $"{_apiUrl}/auth/callback";

		var authData = await _twitchAuth.GetAuthToken(code, redirectUri);

		if (authData.Error != null)
			return new Result<TempSessionToken, TempSessionError> { Error = new TempSessionError(authData.Error.Message) };
		if (authData.Success == null)
			return new Result<TempSessionToken, TempSessionError> { Error = new TempSessionError("Resposta inesperada da Twitch") }; 

		var sessionToken = Guid.NewGuid().ToString();
		
		var tempSession = new TempSessionData(
			authData.Success.AccessToken,
			authData.Success.RefreshToken,
			authData.Success.ExpiresIn
		);

		_cache.Set(sessionToken, tempSession, TimeSpan.FromMinutes(5));
		
		return new Result<TempSessionToken, TempSessionError> { Success = new TempSessionToken(sessionToken) };
	}

	public async Task<Result<UserData, UserError>> GetUserInformationWithSession(string sessionToken)
	{
		var sessionData = _cache.Get<TempSessionData>(sessionToken);

		if (sessionData == null)
			return new Result<UserData, UserError> { Error = new UserError("Sessão inválida ou expirada. Faça login novamente.") };

		string accessToken = sessionData.AccessToken;

		var response = await _twitchApi.GetUserProfile(accessToken);

		if (response.Error != null)
			return new Result<UserData, UserError> { Error = new UserError(response.Error.Message) };
		if (response.Success == null)
			return new Result<UserData, UserError> { Error = new UserError("Erro inesperado ao buscar usuário na Twitch") };
		
		User freshUser;

		var userDataFromTwitchResponse = response.Success;
		var userDataFromDb = await _userRepository.GetProfileByTwitchIdAsync(userDataFromTwitchResponse.Id);
		
		if (userDataFromDb == null)
		{
			freshUser = new User(
				sessionData.AccessToken, 
				sessionData.RefreshToken,
				sessionData.ExpiresIn,
				userDataFromTwitchResponse.Id,
				userDataFromTwitchResponse.Login,
				userDataFromTwitchResponse.DisplayName,
				userDataFromTwitchResponse.ProfileImgUrl
			);

			await _userRepository.AddAsync(freshUser);
		}
		else
		{
			userDataFromDb.AccessToken = sessionData.AccessToken;
			userDataFromDb.RefreshToken = sessionData.RefreshToken;
			userDataFromDb.ExpiresIn = sessionData.ExpiresIn;

			freshUser = userDataFromDb;

			await _userRepository.UpdateAsync(freshUser);
		}

		var newUserData = new UserData(
			freshUser.AccessToken, 
			freshUser.RefreshToken,
			freshUser.ExpiresIn,
			freshUser.TwitchId,
			freshUser.Login,
			freshUser.DisplayName,
			freshUser.ProfileImgUrl,
			freshUser.Id
		);
		
		_cache.Remove(sessionToken);

		return new Result<UserData, UserError> { Success = newUserData };
	}

	public async Task GetUserInformationWithJwt()
	{
		// Decodifica o jwt
		// Pesquisa o usuario
		// Retorna os dados
		// Aqui só vai pegar os dados
	}
}