using System.Text.Json.Serialization;

namespace Application.Common
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }

        public ApiResponse(T data, bool isError, string message)
        {
            Data = data;
            IsError = isError;
            Message = message;
        }

        public ApiResponse() { }

        public static ApiResponse<T> Success(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Data = data,
                IsError = false,
                Message = message
            };
        }

        public static ApiResponse<T> Error(string message, T data = default)
        {
            return new ApiResponse<T>
            {
                Data = data,
                IsError = true,
                Message = message
            };
        }
    }
} 