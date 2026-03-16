using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Integrations;
using SpectraLiveApi.Repositories;

namespace SpectraLiveApi.Services;

public class AuthService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly string _apiUrl;
	private readonly IMemoryCache _cache;
	private readonly IUserRepository _userRepository;

	public AuthService(TwitchAuthClient twitchAuth, IMemoryCache cache, IOptions<SpectraLiveSettings> options, IUserRepository userRepository)
	{
		_twitchAuth = twitchAuth;
		_apiUrl = options.Value.ApiUrl;
		_cache = cache;
		_userRepository = userRepository;
	}

	public async Task<Result<TempSessionToken, TempSessionError>> GetSessionWithTwitchCode(string code)
	{
		var redirectUri = $"{_apiUrl}/auth/callback";

		var authData = await _twitchAuth.GetAuthToken(code, redirectUri);

		if (authData.Error != null)
		{
			return new Result<TempSessionToken, TempSessionError> { Error = new TempSessionError(authData.Error.Message) };
		}

		if (authData.Success == null)
		{
			return new Result<TempSessionToken, TempSessionError> { Error = new TempSessionError("Resposta inesperada da Twitch") };
		} 

		var sessionToken = Guid.NewGuid().ToString();
		
		var tempSession = new TempSessionData(
			authData.Success.AccessToken,
			authData.Success.RefreshToken,
			authData.Success.ExpiresIn
		);

		_cache.Set(sessionToken, tempSession, TimeSpan.FromMinutes(5));
		
		return new Result<TempSessionToken, TempSessionError> { Success = new TempSessionToken(sessionToken) };
	}

	public async Task<Result<TwitchUserResponse, TempSessionError>> GetUserInformationWithSession(string sessionToken)
	{
		// Aqui vai pegar os dados e salvar
		var sessionData = _cache.Get<TempSessionData>(sessionToken);

		if (sessionData == null)
		{
			return new Result<TwitchUserResponse, TempSessionError> { Error = new TempSessionError("Sessão inválida ou expirada. Faça login novamente.") };
		}

		string userToken = sessionData.AccessToken;

		return null;
		// Faz a requisição pra api

		// Verifica se precisa de um refresh

		// Verifica se deu ruim

		// Dado retornado
		
	}

	public async Task GetUserInformationWithJwt()
	{
		// Decodifica o jwt
		// Pesquisa o usuario
		// Retorna os dados
		// Aqui só vai pegar os dados
	}
}