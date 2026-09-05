using System.Text.Json.Serialization;

namespace Application.UseCases.Subastas.Commands.CrearSubasta
{
    public record CrearSubastaCommand(
        int CategoriaId,
        string Titulo,
        string Descripcion,
        string UrlImagen,
        decimal PrecioBase,
        decimal IncrementoMinimo,
        DateTime FechaInicio,
        DateTime FechaFin
    )
    {
        [JsonIgnore]
        public int VendedorId { get; set; }
    }
}
