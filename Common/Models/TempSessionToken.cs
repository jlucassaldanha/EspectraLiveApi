namespace SpectraLiveApi.Common.Models;

public record TempSessionToken(
	string SessionToken
);

public record TempSessionError(string ErrorMessage);

public record TempSessionData(
	string AccessToken,
	string RefreshToken,
	int ExpiresIn
);