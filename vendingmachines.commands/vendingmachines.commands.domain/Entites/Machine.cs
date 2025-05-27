using vendingmachines.commands.cmds;
using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.ValueObjects;

namespace vendingmachines.commands.domain.Entites;

public class Machine : IAggregateRoot
{
    private readonly List<BaseDomainEvent> _events = [];
    private List<Product> products = [];
    public MachineId MachineId { get; set; }
    public MachineType MachineType { get; set; }
    public int Version { get; set; } = -1;

    public Machine()
    {
    }

    public Machine(CreateMachineCommand command)
    {
        RaiseMachineCreatedEvent(new MachineCreatedEvent { EventType = nameof(CreateMachineCommand), MachineType = command.machineType });
    }

    private void RaiseMachineCreatedEvent(MachineCreatedEvent machineCreatedEvent)
    {
        MachineId = new MachineId(Guid.NewGuid());
        MachineType = new MachineType(machineCreatedEvent.MachineType);
        machineCreatedEvent.AggregateId = MachineId.Id.ToString();

        Version++;

        _events.Add(machineCreatedEvent);
    }

    public void AddProduct(ProductId productId, ProductName productName, ProductQty productQty)
    {
        if (products.Count == 10) throw new InvalidOperationException("Machine is full");

        var product = new Product(productId, productName, productQty);

        products.Add(product);

        var productAddedEvent = new ProductAddedEvent
        {
            AggregateId = MachineId.Id.ToString(),
            ProductId = productId.Id.ToString(),
            ProductName = productName.Name,
            ProductQty = productQty.qty,
            EventType = nameof(ProductAddedEvent)
        };

        RaiseProductAddedEvent(productAddedEvent);
    }

    public void OrderProduct(ProductId productId, ProductQty orderQty)
    {
        if (products.Count == 0) throw new InvalidOperationException("Machine is empty");
        
        var product = products.FirstOrDefault(p => p.ProductId.Id.ToString().Equals(productId.Id.ToString()));
        
        if (product == null) throw new InvalidDataException("Product not found");
        
        if (product.ProductQty.qty == 0 || product.ProductQty.qty < orderQty.qty) 
            throw new InvalidOperationException("Not enough stock of this product");
        
        var updatedQty = product.ProductQty.qty - orderQty.qty;
        var prodQty = new ProductQty(updatedQty);
        
        product.ProductQty = prodQty;

        var productOrderedEvent = new ProductOrderedEvent
        {
            AggregateId = MachineId.Id.ToString(),
            OrderedQty = orderQty.qty,
            OrderId = Guid.NewGuid().ToString(),
            ProductId = productId.Id.ToString(),
            EventType = nameof(ProductOrderedEvent)
        };

        RaiseProductOrderedEvent(productOrderedEvent);
    }

    private void RaiseProductOrderedEvent(ProductOrderedEvent productOrderedEvent)
    {
        _events.Add(productOrderedEvent);
        Version++;
    }

    public IReadOnlyList<Product> GetProducts()
    {
        return products.AsReadOnly();
    }

    public void RaiseProductAddedEvent(ProductAddedEvent productAddedEvent)
    {
        _events.Add(productAddedEvent);
        Version++;
    }

    public void RebuildState(List<BaseDomainEvent> events, bool captureEvents = false)
    {
        foreach (var e in events)
        {
            if (captureEvents) _events.Add(e);
            
            if (e is MachineCreatedEvent machineCreatedEvent)
            {
                MachineId = new MachineId(Guid.Parse(machineCreatedEvent.AggregateId));
                MachineType = new MachineType(machineCreatedEvent.MachineType);
            }

            if (e is ProductAddedEvent productAddedEvent)
            {
                var productId = new ProductId(Guid.Parse(productAddedEvent.ProductId));
                var productName = new ProductName(productAddedEvent.ProductName);
                var productQty = new ProductQty(productAddedEvent.ProductQty);

                products.Add(new Product(productId, productName, productQty));
            }

            if (e is ProductQtyUpdatedEvent productQtyUpdatedEvent)
            {
                var product = products.First(p => p.ProductId.Id.ToString().Equals(productQtyUpdatedEvent.ProductId));
                product.ProductQty = new ProductQty(productQtyUpdatedEvent.ProductQty);
            }

            if (e is ProductOrderedEvent productOrderedEvent)
            {
                var product = products.First(p => p.ProductId.Id.ToString().Equals(productOrderedEvent.ProductId));
                product.ProductQty = new ProductQty(product.ProductQty.qty - productOrderedEvent.OrderedQty);
            }
            
            Version++;
        }
    }

    public void EventsCommited()
    {
        _events.Clear();
    }

    public IReadOnlyList<BaseDomainEvent> GetEvents()
    {
        return _events.AsReadOnly();
    }

    public void UpdateProductStock(string productId, int qtyToIncrement)
    {
        var product = products.FirstOrDefault(p => p.ProductId.Id.ToString().Equals(productId));

        if (product == null) throw new InvalidOperationException($"Product with id {productId} is not present in Machine with id {MachineId.Id}");

        var incremented = product.ProductQty.qty + qtyToIncrement;

        product.ProductQty = new ProductQty(incremented);

        RaiseProductQtyUpdatedEvent(new ProductQtyUpdatedEvent
        {
            AggregateId = MachineId.Id.ToString(),
            ProductId = productId,
            ProductQty = incremented,
            Version = Version,
            EventType = nameof(ProductQtyUpdatedEvent)
        });
    }

    private void RaiseProductQtyUpdatedEvent(ProductQtyUpdatedEvent productQtyUpdatedEvent)
    {
        Version++;
        _events.Add(productQtyUpdatedEvent);
    }

    public void ToSnapshot(MachineId machineId, MachineType machineType, int version, List<Product> products)
    {
        MachineId = machineId;
        MachineType = machineType;
        Version = version;
        this.products = products;
    }

    public IReadOnlyList<Product> Products => products.AsReadOnly();
}
