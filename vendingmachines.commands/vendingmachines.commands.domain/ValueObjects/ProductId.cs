namespace vendingmachines.commands.domain.ValueObjects;

public class ProductId
{
    public Guid Id { get; }

    public ProductId(Guid id)
    {
        if (id == Guid.Empty) { throw new ArgumentNullException("invalid id"); }
        Id = id;
    }
}
