using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CrearSubastaRequestDto
    {
        [Required(ErrorMessage = "La categoría es requerida.")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El título es requerido.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La URL de la imagen es requerida.")]
        public string UrlImagen { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio base debe ser mayor a 0.")]
        public decimal PrecioBase { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El incremento mínimo debe ser mayor a 0.")]
        public decimal IncrementoMinimo { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        public DateTime FechaFin { get; set; }
    }
}
