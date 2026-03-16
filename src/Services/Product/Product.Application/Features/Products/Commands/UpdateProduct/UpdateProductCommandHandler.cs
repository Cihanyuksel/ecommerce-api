using MediatR;
using MassTransit; 
using Product.Application.Interfaces;
using Product.Application.Events; 

namespace Product.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint; 

    public UpdateProductCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint) 
    {
        _context = context;
        _publishEndpoint = publishEndpoint; 
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

        if (product == null) return false;

        product.Name = request.Name;
        product.Price = request.Price;
        product.Stock = request.Stock;

        await _context.SaveChangesAsync(cancellationToken);
        
        await _publishEndpoint.Publish(new ProductUpdatedEvent
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        }, cancellationToken);

        return true;
    }
}