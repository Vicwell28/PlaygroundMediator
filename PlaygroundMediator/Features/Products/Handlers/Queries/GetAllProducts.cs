using MediatR;
using PlaygroundMediator.DTOs;

namespace PlaygroundMediator.Features.Products.Handlers
{
    public class GetAllProducts
    {
        // Retorna todos los productos
        public record GetAllProductsQuery()
            : IRequest<ResponseDto<IEnumerable<ProductDto>>>;

        public class GetAllProductsHandler
        : IRequestHandler<GetAllProductsQuery, ResponseDto<IEnumerable<ProductDto>>>
        {
            public async Task<ResponseDto<IEnumerable<ProductDto>>> Handle(
                GetAllProductsQuery request,
                CancellationToken cancellationToken)
            {
                var response = new ResponseDto<IEnumerable<ProductDto>>();

                var products = Enumerable.Range(1, 10) 
                .Select(x => new ProductDto
                {
                    Id = x,
                    Name = $"Product {x}",
                    Price = x * 100m
                }).ToList();

                response.SetSuccess(products, "Products retrieved successfully.");

                return response;
            }
        }
    }
}
