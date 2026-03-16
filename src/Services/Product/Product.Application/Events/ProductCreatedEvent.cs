namespace Product.Application.Events;

public record ProductCreatedEvent
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}