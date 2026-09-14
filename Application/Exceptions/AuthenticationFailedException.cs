namespace Application.Exceptions
{
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message = "Credenciales inválidas.") : base(message)
        {
        }
    }
}
