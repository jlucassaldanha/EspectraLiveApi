namespace SpectraLiveApi.Common;

public record Result<TSuccess, TError>
{
	public TSuccess? Success { get; init; }
	public TError? Error { get; init; }

	public bool IsSuccess => Success != null;
	public bool IsError => Error != null;
}