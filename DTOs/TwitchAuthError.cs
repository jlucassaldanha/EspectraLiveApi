namespace SpectraLiveApi.DTOs;

public record TwitchAuthError(
    string Error,
    int Status,
    string Message
);