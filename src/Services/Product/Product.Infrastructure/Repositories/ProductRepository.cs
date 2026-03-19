using Product.Application.Interfaces.Repositories;
using Product.Infrastructure.Persistence;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<ProductEntity>, IProductRepository
{
    public ProductRepository(ProductDbContext dbContext) : base(dbContext)
    {
    }
}