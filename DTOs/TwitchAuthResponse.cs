using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs;

public record TwitchAuthResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    List<string> Scope,
    [property: JsonPropertyName("token_type")] string TokenType
);

public record TwitchAuthError(
    int Status,
    string Message
);