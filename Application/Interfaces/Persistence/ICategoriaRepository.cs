namespace Application.Interfaces.Persistence
{
    public interface ICategoriaRepository
    {
        Task<bool> ExisteAsync(int id, CancellationToken ct = default);
    }
}
