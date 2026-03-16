namespace SpectraLiveApi.DTOs;

public record TwitchAuthResponse(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    List<string> Scope,
    string TokenType
);