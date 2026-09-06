using Domain.Enums;

namespace Domain.Entities
{
    public class TransaccionLedger
    {
        public int Id { get; private set; }
        public int BilleteraId { get; private set; }
                
        public int? SubastaId { get; private set; }

        public TipoTransaccion Tipo { get; private set; }
        public decimal Monto { get; private set; }
        public DateTime Fecha { get; private set; }

        public Billetera Billetera { get; private set; } = null!;
        public Subasta? Subasta { get; private set; }

        public TransaccionLedger(int billeteraId, TipoTransaccion tipo, decimal monto, int? subastaId = null)
        {
            BilleteraId = billeteraId;
            Tipo = tipo;
            Monto = monto;
            SubastaId = subastaId;
            Fecha = DateTime.UtcNow;
        }

        protected TransaccionLedger() { }
    }
}
