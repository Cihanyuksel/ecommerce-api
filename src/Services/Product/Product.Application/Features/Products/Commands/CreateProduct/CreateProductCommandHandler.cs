using MediatR;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var newProduct = new ProductEntity
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        return newProduct.Id;
    }
}