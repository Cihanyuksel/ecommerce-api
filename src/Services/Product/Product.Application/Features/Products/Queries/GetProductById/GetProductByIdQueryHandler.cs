using MediatR;
using Product.Application.Interfaces.Repositories;
using Product.Domain.Exceptions;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductEntity>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductEntity> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException($"Ürün bulunamadı. (ID: {request.Id})");

        return product;
    }
}