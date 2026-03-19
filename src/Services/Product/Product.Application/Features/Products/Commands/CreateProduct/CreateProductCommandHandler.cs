using MediatR;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces.Repositories;
using Product.Application.Events;
using Shared.Events;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public CreateProductCommandHandler(IProductRepository repository, IPublishEndpoint publishEndpoint, IDistributedCache cache)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var newProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newProduct, cancellationToken);

        await _cache.RemoveAsync("all_products", cancellationToken);

        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock
        }, cancellationToken);

        // INFO 
        await _publishEndpoint.Publish(new LogEventMessage
        {
            ServiceName = "Product.API",
            LogLevel = AppLogLevel.Info,
            Message = $"Admin tarafından yeni ürün eklendi: {newProduct.Name} (Fiyat: {newProduct.Price} TL, Stok: {newProduct.Stock})",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        return newProduct.Id;
    }
}