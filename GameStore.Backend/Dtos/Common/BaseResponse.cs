namespace GameStore.Backend.Dtos.Common
{
    public class BaseResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public object? Errors { get; init; }


        public static BaseResponse<T> Ok(T data, string? message) => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

        public static BaseResponse<T> Fail(string message, object? errors = null) => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
