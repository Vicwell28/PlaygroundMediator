using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlaygroundMediator.DTOs;
using static PlaygroundMediator.Features.Products.Handlers.Commands.CreateProduct;
using static PlaygroundMediator.Features.Products.Handlers.Commands.DeleteProduct;
using static PlaygroundMediator.Features.Products.Handlers.Commands.UpdateProduct;
using static PlaygroundMediator.Features.Products.Handlers.GetAllProducts;
using static PlaygroundMediator.Features.Products.Handlers.GetProductById;
using static PlaygroundMediator.Features.Products.Handlers.Queries.SearchProducts;

namespace PlaygroundMediator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Products
        // Retorna todos los productos
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllProductsQuery();
            var result = await _mediator.Send(query);

            // Si falló, mandamos 400, de lo contrario 200 (OK)
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseDto<ProductDto>), 200)]
        [ProducesResponseType(typeof(ResponseDto<ProductDto>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result); // Producto no encontrado

            return Ok(result);
        }

        // POST: api/Products
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<ProductDto>), 201)]
        [ProducesResponseType(typeof(ResponseDto<ProductDto>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            // Retorna 201 (Created) con la ruta del recurso y el cuerpo con la data
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseDto<bool>), 200)]
        [ProducesResponseType(typeof(ResponseDto<bool>), 400)]
        [ProducesResponseType(typeof(ResponseDto<bool>), 404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
        {
            // Asegurarnos de que el command use el Id de la ruta
            if (id != command.Id)
                return BadRequest("Route Id and command Id do not match.");

            var result = await _mediator.Send(command);

            if (!result.IsSuccess && result.Errors?.Any(e => e.Contains("not found")) == true)
                return NotFound(result);

            if (!result.IsSuccess)
                return BadRequest(result);

            // Si ok, retornamos 200
            return Ok(result);
        }

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseDto<bool>), 200)]
        [ProducesResponseType(typeof(ResponseDto<bool>), 404)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        // GET: api/Products/search?searchTerm=apple&page=1&pageSize=3
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResponseDto<ProductDto>), 200)]
        public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var query = new SearchProductsQuery(searchTerm, page, pageSize);
            var result = await _mediator.Send(query);

            // Aun si no encontró nada, es un 200, pero con una lista vacía 
            // Salvo que hayas decidido que "Sin resultados" es un error, 
            // usualmente no es un error de negocio.
            return Ok(result);
        }
    }
}
