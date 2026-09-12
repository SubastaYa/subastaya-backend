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

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        public Usuario Usuario { get; private set; } = null!;

        public Billetera(int usuarioId, decimal saldoTotal = 0m, decimal saldoRetenido = 0m)
        {
            UsuarioId = usuarioId;
            SaldoTotal = saldoTotal;
            SaldoRetenido = saldoRetenido;
            SaldoDisponible = saldoTotal - saldoRetenido;
        }

        protected Billetera() { }

        // Ingreso de dinero a la billetera
        public void Depositar(decimal monto)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(monto));
            }

            SaldoTotal += monto;
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }

        // Retención temporal en garantía (escrow) mientras la puja sea la más alta
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

        // Liberación de fondos retenidos cuando otro postor supera la oferta
        public void Liberar(decimal monto)
        {
            if (monto <= 0)
            {
                throw new ArgumentException("El monto a liberar debe ser mayor a cero.", nameof(monto));
            }

            SaldoRetenido = Math.Max(0m, SaldoRetenido - monto);
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }

        // Liquidación final para el comprador ganador:
        // El dinero retenido ya sale definitivamente de su cuenta
        public void DebitarRetenido(decimal monto)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto a debitar debe ser mayor a cero.", nameof(monto));
            if (monto > SaldoRetenido)
                throw new InvalidOperationException("El monto a debitar supera el saldo retenido actual.");
            SaldoRetenido -= monto;
            SaldoTotal -= monto;
            SaldoDisponible = SaldoTotal - SaldoRetenido;
        }

        // Acreditación al vendedor de la subasta ganada:
        // Recibe el dinero de la venta como saldo disponible inmediatamente
        public void AcreditarCobro(decimal monto)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto a acreditar debe ser mayor a cero.", nameof(monto));
            SaldoTotal += monto;
            SaldoDisponible += monto;
        }
    }
}
