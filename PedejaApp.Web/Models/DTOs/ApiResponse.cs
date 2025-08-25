using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PedejaApp.Web.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string[]? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse(string error, T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string[] errors, T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
                Error = errors?.FirstOrDefault(),
                Data = data
            };
        }
    }
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResponse(string message = "Operação realizada com sucesso")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse ErrorResponse(string error)
        {
            return new ApiResponse
            {
                Success = false,
                Error = error
            };
        }

        public static ApiResponse ErrorResponse(string[] errors)
        {
            return new ApiResponse
            {
                Success = false,
                Errors = errors,
                Error = errors?.FirstOrDefault()
            };
        }
    }
}
