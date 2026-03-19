using MediatR;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces.Repositories;
using Product.Application.Events;
using Product.Domain.Exceptions;

namespace Product.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public UpdateProductCommandHandler(IProductRepository repository, IPublishEndpoint publishEndpoint, IDistributedCache cache)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException($"Güncellenecek ürün bulunamadı. (ID: {request.Id})");

        product.Name = request.Name;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await _repository.UpdateAsync(product, cancellationToken);

        await _cache.RemoveAsync("all_products", cancellationToken);

        await _publishEndpoint.Publish(new ProductUpdatedEvent
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        }, cancellationToken);

        return true;
    }
}