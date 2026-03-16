namespace SpectraLiveApi.DTOs;

public record TwitchAuthResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    List<string> Scope,
    string TokenType
);

public record TwitchAuthError(
    string Error,
    int Status,
    string Message
);