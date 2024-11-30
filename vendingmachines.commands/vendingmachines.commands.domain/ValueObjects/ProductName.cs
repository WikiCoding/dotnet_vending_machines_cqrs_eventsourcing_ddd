namespace vendingmachines.commands.domain.ValueObjects;

public class ProductName
{
    public string Name { get; }

    public ProductName(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name cannot be null or empty");
        Name = name;
    }
}
