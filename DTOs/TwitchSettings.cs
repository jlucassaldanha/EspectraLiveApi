namespace SpectraLiveApi.DTOs;

public record TwitchSettings{
	public string ClientId { get; init; } = string.Empty;
	public string ClientSecret { get; init; } = string.Empty;
};