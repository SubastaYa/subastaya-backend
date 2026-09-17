
namespace Infrastructure.Services
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public bool Verificar(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
