using Microsoft.EntityFrameworkCore;
using vendingmachines.queries.entities;
using vendingmachines.queries.repository;

namespace vendingmachines.queries.application;

public class ApplicationService
{
    private readonly AppDbContext _context;

    public ApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Machine>> FindAll()
    {
        return await _context.Machines.ToListAsync();
    }

    public async Task<Machine> FindById(string aggId)
    {
        var machine = await _context.Machines.Where(m => m.MachineId.Equals(aggId)).FirstOrDefaultAsync();
        if (machine == null) throw new NullReferenceException("Machine not found");
        return machine;
    }

    public async Task<IEnumerable<Product>> FindProductsByMachineId(string aggId)
    {
        return await _context.Products.Where(p => p.MachineId.Equals(aggId)).ToListAsync();
    }
}
