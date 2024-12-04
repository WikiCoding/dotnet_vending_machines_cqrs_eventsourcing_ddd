using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendingmachines.queries.entities;

public class Machine
{
    [Key]
    [Column("machineid")]
    public string MachineId { get; set; } = string.Empty;
    [Column("machinetype")]
    public string MachineType { get; set; } = string.Empty;
    // public List<Product> Products { get; set; } = [];
    // I can customize with info from the events gotten like when was created and things like that
}
