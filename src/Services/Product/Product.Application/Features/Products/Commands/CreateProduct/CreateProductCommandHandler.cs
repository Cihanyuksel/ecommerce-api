using MediatR;
using Product.Application.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var newProduct = new ProductEntity
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync(cancellationToken);

        return newProduct.Id;
    }
}

