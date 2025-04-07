namespace NewsAggregator.Application.Common
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }

        public static Result<T> Ok(T data) => new() { Success = true, Data = data };
        public static Result<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
    }
}
