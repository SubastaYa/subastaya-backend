namespace Domain.Entities
{
    public class Oferta
    {
        public int Id { get; private set; }
                
        public int SubastaId { get; private set; }
        public int CompradorId { get; private set; }
                
        public decimal Monto { get; private set; }
        public DateTime FechaOferta { get; private set; }

        public Subasta Subasta { get; private set; } = null!;
        public Usuario Comprador { get; private set; } = null!;

                
        public Oferta(int subastaId, int compradorId, decimal monto)
        {
            SubastaId = subastaId;
            CompradorId = compradorId;
            Monto = monto;
            FechaOferta = DateTime.UtcNow;
        }

        protected Oferta() { }
    }
}
