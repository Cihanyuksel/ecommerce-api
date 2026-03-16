using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductEntity>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "all_products";

    public GetProductsQueryHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<ProductEntity>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cachedProducts = await _cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedProducts))
        {
            return JsonSerializer.Deserialize<List<ProductEntity>>(cachedProducts)!;
        }

        var products = await _context.Products
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);

        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));

        var serializedProducts = JsonSerializer.Serialize(products);

        await _cache.SetStringAsync(CacheKey, serializedProducts, cacheOptions, cancellationToken);

        return products;
    }
}