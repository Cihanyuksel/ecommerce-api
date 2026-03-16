namespace Product.Application.Events;

public record ProductDeletedEvent
{
    public Guid Id { get; init; } 
}