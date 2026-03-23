using System.Text.Json.Serialization;

namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchRefreshTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    List<string> Scope,
    [property: JsonPropertyName("token_type")] string TokenType
);