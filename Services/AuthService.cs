using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.Common;
using SpectraLiveApi.Common.Models;
using SpectraLiveApi.Settings;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;
using SpectraLiveApi.Entities;

namespace SpectraLiveApi.Services;

public class AuthService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly TwitchApiClient _twitchApi;
	private readonly string _apiUrl;
	private readonly JwtService _jwtService;
	private readonly IMemoryCache _cache;
	private readonly IUserRepository _userRepository;

	public AuthService(TwitchAuthClient twitchAuth, TwitchApiClient twitchApi, IMemoryCache cache, IOptions<SpectraLiveSettings> options, IUserRepository userRepository, JwtService jwtService)
	{
		_twitchAuth = twitchAuth;
		_twitchApi = twitchApi;
		_apiUrl = options.Value.ApiUrl;
		_cache = cache;
		_jwtService = jwtService;
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

	public async Task<Result<UserData, UserError>> GetUserInformationWithJwt(string authToken)
	{
		var claims = _jwtService.ValidateToken(authToken);
		if (claims == null)
			return new Result<UserData, UserError> { Error = new UserError("JWT inválido") };

		var twitchUserId = claims.FindFirst("twitchId")?.Value;
		var userId = claims.FindFirst("userId")?.Value;
		
		if (string.IsNullOrEmpty(twitchUserId) || string.IsNullOrEmpty(userId))
			return new Result<UserData, UserError> { Error = new UserError("Chaves de usuário não encontradas no JWT") };
		
		User? userDataFromDb = await _userRepository.GetProfileByTwitchIdAsync(twitchUserId);

		if (userDataFromDb == null)
			return new Result<UserData, UserError> { Error = new UserError("Usuário não encontrado no banco de dados.") };

		var userData = new UserData(
			userDataFromDb.AccessToken, 
			userDataFromDb.RefreshToken,
			userDataFromDb.ExpiresIn,
			userDataFromDb.TwitchId,
			userDataFromDb.Login,
			userDataFromDb.DisplayName,
			userDataFromDb.ProfileImgUrl,
			userDataFromDb.Id
		);

		return new Result<UserData, UserError> { Success = userData };
	}
}