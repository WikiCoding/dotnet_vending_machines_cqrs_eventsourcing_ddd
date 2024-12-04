namespace vendingmachines.commands.domain.ValueObjects;

public class ProductQty
{
    public int qty { get; }

    public ProductQty(int qty)
    {
        if (qty < 1 || qty > 10) throw new ArgumentOutOfRangeException(nameof(qty));
        this.qty = qty;
    }
}
