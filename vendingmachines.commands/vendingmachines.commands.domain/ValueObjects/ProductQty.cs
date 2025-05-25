namespace vendingmachines.commands.domain.ValueObjects;

public class ProductQty
{
    public int qty { get; }

    public ProductQty(int qty)
    {
        if (qty < 1) throw new ArgumentOutOfRangeException(nameof(qty));
        if (qty > 10) throw new ArgumentOutOfRangeException($"The stock can't be bigger than 10 but it was {qty}");
        this.qty = qty;
    }
}
