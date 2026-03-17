using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs;

public record TwitchRefreshTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    List<string> Scope,
    [property: JsonPropertyName("token_type")] string TokenType
);

public record TwitchRefreshTokenError(
    string Error,
    int Status,
    string Message
);