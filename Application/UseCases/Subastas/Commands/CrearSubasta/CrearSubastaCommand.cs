using System.Text.Json.Serialization;
using Application.Interfaces;

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
    ) : ICommand<int>
    {
        [JsonIgnore]
        public int VendedorId { get; set; }
    }
}
