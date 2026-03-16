namespace SpectraLiveApi.DTOs;

public record TwitchRefreshTokenError(
    string Error,
    int Status,
    string Message
);