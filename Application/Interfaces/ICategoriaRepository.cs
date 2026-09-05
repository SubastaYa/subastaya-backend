namespace Application.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<bool> ExisteAsync(int id, CancellationToken ct = default);
    }
}
