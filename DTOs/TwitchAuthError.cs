namespace SpectraLiveApi.DTOs;

public record TwitchAuthError(
    int Status,
    string Message
);