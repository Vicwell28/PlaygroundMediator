namespace PlaygroundMediator.DTOs
{
    public class BaseResponseDto
    {
        public bool IsSuccess => Errors == null || !Errors.Any();
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public string? Code { get; set; }
        public int StatusCode { get; set; } = 200;

        public virtual void SetSuccess(string? message = null, int? statusCode = null, string? code = null)
        {
            Message = message;
            Errors = null;
            Code = code;
            StatusCode = statusCode ?? 200;
        }

        public virtual void SetError(string? message, List<string>? errors = null, int? statusCode = null, string? code = null)
        {
            Message = message;
            Errors = errors ?? new List<string>();
            Code = code;
            StatusCode = statusCode ?? 200;
        }
    }
}
