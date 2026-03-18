using System.Net;

namespace SpectraLiveApi.Common;

public record Error(string Message, HttpStatusCode ErrorCode = HttpStatusCode.BadRequest);

public record Result<T>
{
	public T? Data { get; }
	public Error? Error { get; }
	public bool IsSuccess => Error == null;
	public bool IsError => Error != null;

	private Result(T? data, Error? error)
	{
		Data = data;
		Error = error;
	}

	public static Result<T> Success(T data) => new (data, null);
	public static Result<T> Failure(Error error) => new (default, null);
}

/*public record Result<TSuccess, TError>
{
	public TSuccess? Success { get; init; }
	public TError? Error { get; init; }

	public bool IsSuccess => Success != null;
	public bool IsError => Error != null;
}*/