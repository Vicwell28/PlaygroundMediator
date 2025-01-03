using MediatR;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.PipelineBehavior;

namespace PlaygroundMediator.Features.Products.Handlers.Commands
{
    public class DeleteProduct
    {
        // Elimina un producto, retorna un ResponseDto<bool>
        public record DeleteProductCommand(int Id)
            : IRequest<ResponseDto<bool>>, IRequireValidation;

        public class DeleteProductHandler
        : IRequestHandler<DeleteProductCommand, ResponseDto<bool>>
        {
            public async Task<ResponseDto<bool>> Handle(
                DeleteProductCommand request,
                CancellationToken cancellationToken)
            {
                var response = new ResponseDto<bool>();

                response.SetSuccess(true, "Product deleted successfully.");

                return response;
            }
        }
    }
}
