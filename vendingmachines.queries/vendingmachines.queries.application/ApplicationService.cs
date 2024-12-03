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
}
