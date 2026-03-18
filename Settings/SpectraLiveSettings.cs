namespace SpectraLiveApi.Settings;

public record SpectraLiveSettings{
	public string ApiUrl { get; init; } = string.Empty;
    public string FrontendUrl { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string Algorithm { get; init; } = string.Empty;
};