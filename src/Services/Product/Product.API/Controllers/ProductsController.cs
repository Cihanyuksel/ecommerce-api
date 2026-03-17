using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Queries.GetProducts;
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
        return Created("", new { id = productId, message = "Ürün başarıyla oluşturuldu!" });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);

        return Ok(products);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound("Güncellenecek ürün bulunamadı.");

        return Ok(new { message = "Ürün başarıyla güncellendi!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound("Silinecek ürün bulunamadı.");

        return Ok(new { message = "Ürün başarıyla silindi!" });
    }
}

