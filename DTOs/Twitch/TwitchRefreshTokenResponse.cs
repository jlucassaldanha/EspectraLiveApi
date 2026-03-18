namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchRefreshTokenResponse(
    string Error,
    int Status,
    string Message
);