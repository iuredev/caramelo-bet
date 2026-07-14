using System.Text.Json.Serialization;

namespace CarameloBet.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, string code) => new()
    {
        Success = false,
        Error = new ApiError(message, code)
    };

}

public record ApiError(string Message, string Code);
