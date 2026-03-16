namespace SpectraLiveApi.DTOs;

public record TempSessionData(
	string AccessToken,
	string RefreshToken,
	int ExpiresIn
);