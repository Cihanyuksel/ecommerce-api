using MediatR;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces.Repositories;
using Product.Application.Events;
using Product.Domain.Exceptions;

namespace Product.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public DeleteProductCommandHandler(IProductRepository repository, IPublishEndpoint publishEndpoint, IDistributedCache cache)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException($"Silinecek ürün bulunamadı. (ID: {request.Id})");

        await _repository.DeleteAsync(product, cancellationToken);

        await _cache.RemoveAsync("all_products", cancellationToken);

        await _publishEndpoint.Publish(new ProductDeletedEvent { Id = product.Id }, cancellationToken);

        return true;
    }
}