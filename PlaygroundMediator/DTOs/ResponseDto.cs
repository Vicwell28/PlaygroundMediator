namespace PlaygroundMediator.DTOs
{
    public class ResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }

        public virtual void SetSuccess(T data, string? message = null, int? statusCode = null, string? code = null)
        {
            base.SetSuccess(message, statusCode, code);

            Data = data;
        }
    }
}
