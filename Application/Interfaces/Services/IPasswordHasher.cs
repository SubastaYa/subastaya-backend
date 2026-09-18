namespace Application.Interfaces.Services
{
    public interface IPasswordHasher
    {
        bool Verificar(string password, string passwordHash);
        string Hash(string password);
    }
}
