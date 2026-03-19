namespace SpectraLiveApi.Common.Models;

public record TwitchAuthData(
	string AccessToken,
	string RefreshToken,
	int ExpiresIn
);