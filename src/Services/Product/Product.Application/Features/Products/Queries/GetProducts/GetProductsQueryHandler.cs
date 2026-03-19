using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces.Repositories;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductEntity>>
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "all_products";

    public GetProductsQueryHandler(IProductRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<ProductEntity>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cachedProducts = await _cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedProducts))
        {
            return JsonSerializer.Deserialize<List<ProductEntity>>(cachedProducts)!;
        }

        var products = await _repository.GetAllAsync(cancellationToken);

        var productList = products.ToList();

        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));

        var serializedProducts = JsonSerializer.Serialize(productList);

        await _cache.SetStringAsync(CacheKey, serializedProducts, cacheOptions, cancellationToken);

        return productList;
    }
}