using MediatR;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.PipelineBehavior;

namespace PlaygroundMediator.Features.Products.Handlers.Commands
{
    public class UpdateProduct
    {
        // Actualiza un producto, retorna un ResponseDto<bool> indicando si se actualizó
        public record UpdateProductCommand(int Id, string Name, decimal Price)
            : IRequest<ResponseDto<bool>>, IRequireValidation;

        public class UpdateProductHandler
            : IRequestHandler<UpdateProductCommand, ResponseDto<bool>>
        {
            public async Task<ResponseDto<bool>> Handle(
                UpdateProductCommand request,
                CancellationToken cancellationToken)
            {
                var response = new ResponseDto<bool>();

                response.SetSuccess(true, "Product updated successfully.");

                return response;
            }
        }
    }
}

