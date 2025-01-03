using MediatR;
using PlaygroundMediator.DTOs;

namespace PlaygroundMediator.PipelineBehavior
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : BaseResponseDto, new()
    {
        private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

        public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                // Ejecuta el siguiente Behavior o el Handler final
                var response = await next();
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción en {RequestName}", typeof(TRequest).Name);

                // Como TResponse hereda de BaseResponseDto, podemos construir uno
                var errorResponse = new TResponse();

                errorResponse.SetError(
                    message: "Error durante la ejecución del request",
                    errors: new List<string> { ex.Message },
                    code: "ERR-UNHANDLED"
                );

                return errorResponse;
            }
        }
    }
}
