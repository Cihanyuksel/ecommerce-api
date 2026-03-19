using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Interfaces.Repositories;

public interface IProductRepository : IAsyncRepository<ProductEntity>
{
    
}