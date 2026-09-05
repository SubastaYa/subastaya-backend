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
    }
}
