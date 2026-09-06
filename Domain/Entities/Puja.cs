namespace Domain.Entities
{
    public class Puja
    {
        public int Id { get; private set; }
                
        public int SubastaId { get; private set; }
        public int CompradorId { get; private set; }
                
        public decimal Monto { get; private set; }
        public DateTime FechaPuja { get; private set; }

        public Subasta Subasta { get; private set; } = null!;
        public Usuario Comprador { get; private set; } = null!;
                
        public Puja(int subastaId, int compradorId, decimal monto)
        {
            SubastaId = subastaId;
            CompradorId = compradorId;
            Monto = monto;
            FechaPuja = DateTime.UtcNow;
        }

        protected Puja() { }
    }
}
