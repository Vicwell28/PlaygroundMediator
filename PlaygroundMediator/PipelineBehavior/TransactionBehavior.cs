using MediatR;

namespace PlaygroundMediator.PipelineBehavior
{
    //public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    //{
    //    private readonly DbContext _dbContext;

    //    public TransactionBehavior(DbContext dbContext)
    //    {
    //        _dbContext = dbContext;
    //    }

    //    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    //    {
    //        // Iniciar la transacción
    //        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

    //        try
    //        {
    //            var response = await next();

    //            // Confirmar transacción
    //            await transaction.CommitAsync(cancellationToken);

    //            return response;
    //        }
    //        catch
    //        {
    //            // Rollback
    //            await transaction.RollbackAsync(cancellationToken);

    //            throw;
    //        }
    //    }
    //}
}
