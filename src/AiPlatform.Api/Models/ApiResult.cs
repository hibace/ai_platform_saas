namespace AiPlatform.Api.Models;

/// <summary>
/// Universal API response in Result pattern style.
/// UI checks isSuccess and uses data or error.
/// </summary>
public sealed class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }

    private ApiResult(bool isSuccess, T? data, string? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static ApiResult<T> Success(T data) => new(true, data, null);
    public static ApiResult<T> Failure(string error) => new(false, default, error);
}
