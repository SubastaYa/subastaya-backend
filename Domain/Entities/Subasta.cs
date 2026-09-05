using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
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
                
        public Subasta(int vendedorId, int categoriaId, string titulo, string descripcion, string urlImagen, decimal precioBase, decimal incrementoMinimo, DateTime fechaInicio, DateTime fechaFin, EstadoSubasta estado = EstadoSubasta.Programada)
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
            Estado = estado;
        }

        public static Subasta Crear(
            int vendedorId,
            int categoriaId,
            string titulo,
            string descripcion,
            string urlImagen,
            decimal precioBase,
            decimal incrementoMinimo,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DomainValidationException("El título es obligatorio.");

            if (string.IsNullOrWhiteSpace(urlImagen))
                throw new DomainValidationException("La URL de la imagen es obligatoria.");

            if (precioBase <= 0)
                throw new DomainValidationException("El precio base debe ser un valor positivo mayor a cero.");

            if (incrementoMinimo <= 0)
                throw new DomainValidationException("El incremento mínimo debe ser un valor positivo mayor a cero.");

            if (fechaFin <= fechaInicio)
                throw new DomainValidationException("La fecha de fin debe ser posterior a la fecha de inicio.");

            var ahora = DateTime.UtcNow;
            var estadoInicial = fechaInicio <= ahora ? EstadoSubasta.Activa : EstadoSubasta.Programada;

            return new Subasta(
                vendedorId,
                categoriaId,
                titulo,
                descripcion,
                urlImagen,
                precioBase,
                incrementoMinimo,
                fechaInicio,
                fechaFin,
                estadoInicial
            );
        }

        public void ExtenderFechaFin(int minutos = 2)
        {
            FechaFin = FechaFin.AddMinutes(minutos);
        }

        public void IncrementarVersion()
        {
            Version++;
        }

        protected Subasta() { }
    }
}
