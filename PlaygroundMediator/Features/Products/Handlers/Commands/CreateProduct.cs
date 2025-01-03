using FluentValidation;
using MediatR;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.PipelineBehavior;

namespace PlaygroundMediator.Features.Products.Handlers.Commands
{
    public class CreateProduct
    {
        // Crea un producto, retorna un ResponseDto<ProductDto> para unificado
        public record CreateProductCommand(string Name, decimal Price) 
            : IRequest<ResponseDto<ProductDto>>, IRequireValidation;


        public class CreateProductHandler
            : IRequestHandler<CreateProductCommand, ResponseDto<ProductDto>>
        {
            public async Task<ResponseDto<ProductDto>> Handle(
                CreateProductCommand request,
                CancellationToken cancellationToken)
            {
                var response = new ResponseDto<ProductDto>();

                // Crear nuevo producto
                var newProduct = new ProductDto
                {
                    Name = request.Name,
                    Price = request.Price
                };

                // Retornar con el dto final
                response.SetSuccess(newProduct, "Product created successfully.");
                return response;
            }
        }

        public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
        {
            public CreateProductCommandValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required.")
                    .MaximumLength(100).WithMessage("Name can't exceed 100 characters.");

                RuleFor(x => x.Price)
                    .NotEmpty().WithMessage("Price is required.")
                    .GreaterThan(0).WithMessage("Price must be greater than 0.");
            }
        }

    }

}
