using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Subasta
    {
        public int Id { get; private set; }
        public int VendedorId { get; private set; }
        public int CategoriaId { get; private set; }

        public string Titulo { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        public string UrlImagen { get; private set; } = string.Empty;

        public decimal PrecioBase { get; private set; }
        public decimal IncrementoMinimo { get; private set; }

        public DateTime FechaInicio { get; private set; }
        public DateTime FechaFin { get; private set; }

        public EstadoSubasta Estado { get; private set; }
        // Concurrencia optimista: SQL Server actualiza este token binario en cada UPDATE
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        public Usuario Vendedor { get; private set; } = null!;
        public Categoria Categoria { get; private set; } = null!;

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

        // Regla anti-sniping: extiende el cierre para permitir contraofertas de último momento
        public void ExtenderFechaFin(int minutos = 2)
        {
            FechaFin = FechaFin.AddMinutes(minutos);
        }

        // Monto mínimo exigido: si ya hay ofertas, la mayor + incremento; de lo contrario el precio base
        public decimal ObtenerMontoMinimoRequerido()
        {
            return Pujas.Count > 0
                ? Pujas.Max(p => p.Monto) + IncrementoMinimo
                : PrecioBase;
        }

        // Valida que la subasta esté activa, no vencida y que el monto cubra el incremento exigido
        public void ValidarPuedeRecibirPuja(decimal monto)
        {
            if (Estado != EstadoSubasta.Activa)
            {
                throw new SubastaNoActivaException("La subasta no se encuentra activa para recibir ofertas.");
            }

            if (DateTime.UtcNow > FechaFin)
            {
                throw new SubastaVencidaException("La subasta ya ha finalizado.");
            }

            decimal montoMinimo = ObtenerMontoMinimoRequerido();
            if (monto < montoMinimo)
            {
                throw new PujaInvalidaException($"El monto de la oferta ({monto}) debe ser mayor o igual al mínimo requerido ({montoMinimo}).");
            }
        }

        public void Finalizar()
        {
            if (Estado != EstadoSubasta.Activa)
                throw new DomainValidationException("Solo se pueden finalizar subastas que estén activas.");
            Estado = EstadoSubasta.Finalizada;
        }

        public void MarcarDesierta()
        {
            if (Estado != EstadoSubasta.Activa)
                throw new DomainValidationException("Solo se pueden marcar como desiertas subastas activas.");
            if (Pujas.Any())
                throw new DomainValidationException("No se puede marcar como desierta una subasta con ofertas.");
            Estado = EstadoSubasta.Desierta;
        }

        public void Activar()
        {
            if (Estado != EstadoSubasta.Programada)
                throw new DomainValidationException("Solo subastas programadas pueden pasar a activas.");
            Estado = EstadoSubasta.Activa;
        }

        protected Subasta() { }
    }
}
