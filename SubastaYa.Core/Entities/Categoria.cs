using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.Entities
{
    public class Categoria
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public string UrlIcono { get; private set; }
                       
        public IReadOnlyCollection<Subasta> Subastas { get; private set; } = new List<Subasta>();
                
        public Categoria(string nombre, string urlIcono)
        {
            Nombre = nombre;
            UrlIcono = urlIcono;
        }
               
        protected Categoria() { }
    }
}
