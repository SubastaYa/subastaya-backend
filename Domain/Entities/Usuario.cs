namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string Nombre { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime FechaRegistro { get; private set; }
                
        public Billetera? Billetera { get; private set; }
                
        public Usuario(string email, string nombre, string passwordHash)
        {
            Email = email;
            Nombre = nombre;
            PasswordHash = passwordHash;
            FechaRegistro = DateTime.UtcNow;
        }
                
        protected Usuario() { }
    }
}
