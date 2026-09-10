namespace Domain.Entities
{
    public class Categoria
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string UrlIcono { get; private set; } = string.Empty;
                       
        public IReadOnlyCollection<Subasta> Subastas { get; private set; } = new List<Subasta>();
                
        public Categoria(string nombre, string urlIcono)
        {
            Nombre = nombre;
            UrlIcono = urlIcono;
        }
               
        protected Categoria() { }
    }
}
