using MediatR;
using PlaygroundMediator.DTOs;

namespace PlaygroundMediator.Features.Products.Handlers
{
    public class GetProductById
    {
        // Retorna producto por ID
        public record GetProductByIdQuery(int Id)
            : IRequest<ResponseDto<ProductDto>>;

        public class GetProductByIdHandler
        : IRequestHandler<GetProductByIdQuery, ResponseDto<ProductDto>>
        {
            public async Task<ResponseDto<ProductDto>> Handle(
                GetProductByIdQuery request,
                CancellationToken cancellationToken)
            {
                var response = new ResponseDto<ProductDto>();

                var product = new ProductDto
                {
                    Id = request.Id,
                    Name = $"Product {request.Id}",
                    Price = request.Id * 100m
                };

                if (product == null)
                {
                    response.SetError($"Product with Id {request.Id} not found.", code: "ERR-PROD-NOTFOUND");
                    return response;
                }

                response.SetSuccess(product, "Product retrieved successfully.");

                return response;
            }
        }
    }
}
