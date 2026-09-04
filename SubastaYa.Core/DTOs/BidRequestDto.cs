using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Core.DTOs
{
    public class BidRequestDto
    {
        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la puja debe ser mayor a 0.")]
        public decimal Amount { get; set; }
    }
}
