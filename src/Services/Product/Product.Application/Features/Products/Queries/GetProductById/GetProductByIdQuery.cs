using MediatR;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductEntity>
{
    public Guid Id { get; set; }
}