using MediatR;
using PlaygroundMediator.DTOs;

namespace PlaygroundMediator.Features.Products.Handlers.Queries
{
    public class SearchProducts
    {
        // Búsqueda con paginación
        public record SearchProductsQuery(
            string? SearchTerm,
            int Page = 1,
            int PageSize = 5)
            : IRequest<SearchResponseDto<ProductDto>>;

        public class SearchProductsHandler
        : IRequestHandler<SearchProductsQuery, SearchResponseDto<ProductDto>>
        {
            public async Task<SearchResponseDto<ProductDto>> Handle(
                SearchProductsQuery request,
                CancellationToken cancellationToken)
            {
                var response = new SearchResponseDto<ProductDto>();

                var products = new List<ProductDto>() {
                    new ProductDto { Id = 1, Name = "Product 1", Price = 100m },
                    new ProductDto { Id = 2, Name = "Product 2", Price = 200m },
                    new ProductDto { Id = 3, Name = "Product 3", Price = 300m },
                    new ProductDto { Id = 4, Name = "Product 4", Price = 400m },
                    new ProductDto { Id = 5, Name = "Product 5", Price = 500m }
                };

                // Realizar búsqueda en el repo
                var (items, total) = (products, products.Count);

                // Setear datos de paginación
                response.SetSuccess(items, "Products searched successfully.");
                response.SetPaginationData(request.Page, request.PageSize, total);

                // Info adicional de búsqueda
                response.SearchTermUsed = request.SearchTerm;
                response.AppliedFilters = new Dictionary<string, string>
                {
                    { "SearchTerm", request.SearchTerm ?? string.Empty }
                };

                return response;
            }
        }
    }
}
