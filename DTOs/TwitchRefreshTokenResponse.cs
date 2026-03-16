namespace SpectraLiveApi.DTOs;

public record TwitchRefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    List<string> Scope,
    string TokenType
);

public record TwitchRefreshTokenError(
    string Error,
    int Status,
    string Message
);