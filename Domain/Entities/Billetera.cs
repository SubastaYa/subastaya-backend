using Domain.Exceptions;

namespace Domain.Entities
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
                
        public Billetera(int usuarioId, decimal saldoTotal = 0m, decimal saldoRetenido = 0m)
        {
            UsuarioId = usuarioId;
            SaldoTotal = saldoTotal;
            SaldoRetenido = saldoRetenido;
            SaldoDisponible = saldoTotal - saldoRetenido;
        }

        protected Billetera() { }

        public void Depositar(decimal monto)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(monto));
            }

            SaldoTotal += monto;
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }

        public void Retener(decimal monto)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a retener debe ser mayor a cero.", nameof(monto));
            }

            if (monto > SaldoDisponible)
            {
                throw new SaldoInsuficienteException($"Saldo disponible insuficiente para retener {monto:F2}. Saldo disponible actual: {SaldoDisponible:F2}.");
            }

            SaldoRetenido += monto;
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }

        public void Liberar(decimal monto)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a liberar debe ser mayor a cero.", nameof(monto));
            }

            SaldoRetenido = Math.Max(0m, SaldoRetenido - monto);
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }
    }
}
