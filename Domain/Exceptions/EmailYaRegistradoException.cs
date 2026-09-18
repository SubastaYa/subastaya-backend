namespace Domain.Exceptions
{
    public class EmailYaRegistradoException : Exception
    {
        public EmailYaRegistradoException(string email)
            : base($"El correo electrónico '{email}' ya está en uso.")
        {
        }

        public EmailYaRegistradoException()
            : base("El correo electrónico ya está en uso.")
        {
        }
    }
}
