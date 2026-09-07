using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class PujaRepository : IPujaRepository
    {
        private readonly ApplicationDbContext _context;

        public PujaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Puja puja, CancellationToken ct = default)
        {
            await _context.Pujas.AddAsync(puja, ct);
        }
    }
}
