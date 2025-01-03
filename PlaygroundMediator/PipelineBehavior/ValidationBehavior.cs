using FluentValidation;
using MediatR;

namespace PlaygroundMediator.PipelineBehavior
{
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IRequireValidation
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Si no hay validadores registrados para este TRequest, pasa directamente al siguiente
            if (!_validators.Any())
                return await next();

            // Ejecutar todas las validaciones
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // Combinar errores de todos los validadores
            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            // Si hay errores, podemos lanzar una excepción (o manejarlo de otra forma)
            if (failures.Any())
            {
                // Ejemplo: lanzar una excepción de validación propia 
                throw new ValidationException(failures);
            }

            // Si todo está correcto, continúa la ejecución con el siguiente comportamiento/handler
            return await next();
        }
    }
}
