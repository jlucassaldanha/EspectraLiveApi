using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SpectraLiveApi.DTOs;
using SpectraLiveApi.Integrations;

namespace SpectraLiveApi.Services;

public class AuthService
{
	private readonly TwitchAuthClient _twitchAuth;
	private readonly string _apiUrl;
	private readonly IMemoryCache _cache;

	public AuthService(TwitchAuthClient twitchAuth, IMemoryCache cache, IOptions<SpectraLiveSettings> options)
	{
		_twitchAuth = twitchAuth;
		_apiUrl = options.Value.ApiUrl;
		_cache = cache;
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
}