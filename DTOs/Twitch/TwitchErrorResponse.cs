namespace SpectraLiveApi.DTOs.Twitch;

public record TwitchErrorResponse(
    string? Error,
    int? Status,
    string? Message
);