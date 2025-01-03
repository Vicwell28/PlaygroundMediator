using MediatR;
using System.Diagnostics;

namespace PlaygroundMediator.PipelineBehavior
{
    public class LoggingBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly Stopwatch _stopwatch = new();

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _stopwatch.Start();

            _logger.LogInformation("Handling {RequestName} with payload {@Request}", typeof(TRequest).Name, request);

            var response = await next();

            _stopwatch.Stop();

            _logger.LogInformation("Handled {RequestName} -> {ResponseName} in {ElapsedMilliseconds} ms",
                                   typeof(TRequest).Name,
                                   typeof(TResponse).Name,
                                   _stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}
