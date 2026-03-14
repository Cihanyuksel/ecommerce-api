namespace Product.Domain.Entities;

public class Product
{
    public Guid Id {get; set;}

    public string Name {get; set;} = string.Empty;

    public decimal Price {get; set;} 

    public int Stock {get; set;}

    public DateTime CreatedAt { get; set; }

    public Product()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}

