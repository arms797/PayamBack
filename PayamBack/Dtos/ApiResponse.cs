namespace PayamBack.Dtos
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse Success(object? data = null, string message = "عملیات با موفقیت انجام شد")
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static ApiResponse Error(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static ApiResponse ServerError(string message = "خطای داخلی سرور. لطفاً با پشتیبانی تماس بگیرید.")
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                StatusCode = 500
            };
        }
    }
}