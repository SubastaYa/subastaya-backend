using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BidRequestDto
    {
        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la oferta debe ser mayor a 0.")]
        public decimal Amount { get; set; }
    }
}
