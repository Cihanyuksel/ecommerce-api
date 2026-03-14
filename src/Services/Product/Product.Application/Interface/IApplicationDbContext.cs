using Microsoft.EntityFrameworkCore;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ProductEntity> Products {get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}