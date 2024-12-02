using vendingmachines.commands.contracts;
using vendingmachines.commands.domain.DDD;
using vendingmachines.commands.domain.DomainEvents;
using vendingmachines.commands.domain.ValueObjects;

namespace vendingmachines.commands.domain.Entites;

public class Machine : IAggregateRoot
{
    private readonly List<BaseDomainEvent> _events = [];
    private readonly List<Product> products = [];
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

    public void RaiseMachineCreatedEvent(MachineCreatedEvent machineCreatedEvent)
    {
        MachineId = new MachineId(Guid.NewGuid());
        MachineType = new MachineType(machineCreatedEvent.MachineType);
        machineCreatedEvent.AggregateId = MachineId.Id.ToString();

        Version++;

        _events.Add(machineCreatedEvent);
    }

    public void AddProduct(ProductId productId, ProductName productName, ProductQty productQty)
    {
        if (products.Count == 10)
        {
            throw new InvalidOperationException("Machine is full");
        }

        var product = new Product(productId, productName, productQty);

        products.Add(product);

        var productAddedEvent = new ProductAddedEvent
        {
            AggregateId = MachineId.Id.ToString(),
            ProductId = productId.Id.ToString(),
            ProductName = productName.Name,
            ProductQty = productQty.qty
        };

        RaiseProductAddedEvent(productAddedEvent);
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

    public void RebuildState(List<BaseDomainEvent> events)
    {
        foreach (var e in events)
        {
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
                var product = products.Where(p => p.ProductId.Id.ToString().Equals(productQtyUpdatedEvent.ProductId)).First();
                product.ProductQty = new ProductQty(productQtyUpdatedEvent.ProductQty);
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

    public void UpdateProductStock(string productId, int qtyToIcrement)
    {
        var product = products.Where(p => p.ProductId.Id.ToString().Equals(productId)).FirstOrDefault();

        if (product == null) throw new InvalidOperationException($"Product with id {productId} is not present in Machine with id {MachineId.Id}");

        var incremented = product.ProductQty.qty + qtyToIcrement;

        product.ProductQty = new ProductQty(incremented);

        RaiseProductQtyUpdatedEvent(new ProductQtyUpdatedEvent
        {
            AggregateId = MachineId.Id.ToString(),
            ProductId = productId,
            ProductQty = incremented,
            Version = Version,
        });
    }

    private void RaiseProductQtyUpdatedEvent(ProductQtyUpdatedEvent productQtyUpdatedEvent)
    {
        Version++;
        _events.Add(productQtyUpdatedEvent);
    }

    public IReadOnlyList<Product> Products => products.AsReadOnly();
}
