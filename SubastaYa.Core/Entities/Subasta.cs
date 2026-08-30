using SubastaYa.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.Entities
{
    public class Subasta
    {
        public int Id { get; private set; }
        public int VendedorId { get; private set; }
        public int CategoriaId { get; private set; }

        public string Titulo { get; private set; }
        public string Descripcion { get; private set; }
        public string UrlImagen { get; private set; }

        public decimal PrecioBase { get; private set; }
        public decimal IncrementoMinimo { get; private set; }

        public DateTime FechaInicio { get; private set; }
        public DateTime FechaFin { get; private set; }

        public EstadoSubasta Estado { get; private set; }
        public int Version { get; private set; }
                

        public Usuario Vendedor { get; private set; }
        public Categoria Categoria { get; private set; }
        
        public IReadOnlyCollection<Puja> Pujas { get; private set; } = new List<Puja>();
                
        public Subasta(int vendedorId, int categoriaId, string titulo, string descripcion, string urlImagen, decimal precioBase, decimal incrementoMinimo, DateTime fechaInicio, DateTime fechaFin)
        {
            VendedorId = vendedorId;
            CategoriaId = categoriaId;
            Titulo = titulo;
            Descripcion = descripcion;
            UrlImagen = urlImagen;
            PrecioBase = precioBase;
            IncrementoMinimo = incrementoMinimo;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            Estado = EstadoSubasta.Programada;
        }
                
        protected Subasta() { }
    }
}
