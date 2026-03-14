using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductEntity>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductEntity>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);

        return products;
    }
}