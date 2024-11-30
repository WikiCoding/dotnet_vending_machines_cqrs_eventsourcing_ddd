using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.domain.ValueObjects;

namespace vendingmachines.commands.domain.Entites;

public class Product : IEntity
{
    public ProductId ProductId { get; set; }
    public ProductName ProductName { get; set; }
    public ProductQty ProductQty { get; set; }

    public Product(ProductId productId, ProductName productName, ProductQty productQty)
    {
        ProductId = productId;
        ProductName = productName;
        ProductQty = productQty;
    }
}
