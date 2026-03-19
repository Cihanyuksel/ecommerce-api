using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Queries.GetProducts;
using Product.Application.Features.Products.Queries.GetProductById;
using Product.Application.Features.Products.Commands.UpdateProduct;
using Product.Application.Features.Products.Commands.DeleteProduct;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var productId = await _mediator.Send(command);
        return Created(" ", new { id = productId, message = "Ürün başarıyla oluşturuldu!" });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);

        return Ok(products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var product = await _mediator.Send(query);

        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;

        await _mediator.Send(command);
        return Ok(new { message = "Ürün başarıyla güncellendi!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id });
        return Ok(new { message = "Ürün başarıyla silindi!" });
    }
}