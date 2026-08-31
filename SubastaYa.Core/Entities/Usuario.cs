using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.Entities
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string Nombre { get; private set; }
        public string PasswordHash { get; private set; }
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
