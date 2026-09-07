using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteAsync(int id, CancellationToken ct = default)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id, ct);
        }
    }
}
