using MediatR;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Application.Features.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<List<ProductEntity>>
{
}