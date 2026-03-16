using MediatR;
using MassTransit; 
using Product.Application.Interfaces;
using Product.Application.Events; 
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint; 

    public CreateProductCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint) // DEĞİŞTİRİLDİ
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
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

        var productCreatedEvent = new ProductCreatedEvent
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock
        };

        await _publishEndpoint.Publish(productCreatedEvent, cancellationToken);

        return newProduct.Id;
    }
}