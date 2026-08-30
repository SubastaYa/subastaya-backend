using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.Entities
{
    public class Billetera
    {
        public int Id { get; private set; }
        public int UsuarioId { get; private set; }
        public decimal SaldoTotal { get; private set; }
        public decimal SaldoRetenido { get; private set; }
        public decimal SaldoDisponible { get; private set; }
                
        public int Version { get; private set; }

        public Usuario Usuario { get; private set; }
                
        public Billetera(int usuarioId)
        {
            UsuarioId = usuarioId;
            SaldoTotal = 0m;
            SaldoRetenido = 0m;
            SaldoDisponible = 0m;
        }

        protected Billetera() { }
    }
}
