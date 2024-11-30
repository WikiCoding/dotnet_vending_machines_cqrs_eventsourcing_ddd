namespace vendingmachines.commands.domain.ValueObjects;

public class ProductQty
{
    public int qty { get; }

    public ProductQty(int qty)
    {
        if (qty < 0) throw new ArgumentOutOfRangeException(nameof(qty));
        this.qty = qty;
    }
}
