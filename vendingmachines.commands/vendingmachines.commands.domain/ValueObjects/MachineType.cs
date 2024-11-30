namespace vendingmachines.commands.domain.ValueObjects;

public class MachineType
{
    public string Type { get; }

    public MachineType(string type)
    {
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException("type cannot be null or empty");
        Type = type;
    }
}
