namespace PedejaApp.Web.Models.DTOs
{
    public class ResultDto<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }

        public static ResultDto<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ResultDto<T> Fail(string error) => new() { IsSuccess = false, Error = error };
    }
}
