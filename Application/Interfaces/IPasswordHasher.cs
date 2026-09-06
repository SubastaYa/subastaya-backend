namespace Application.Interfaces
{
    public interface IPasswordHasher
    {
        bool Verificar(string password, string passwordHash);
    }
}
