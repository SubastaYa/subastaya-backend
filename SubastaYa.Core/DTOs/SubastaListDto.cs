using SubastaYa.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.DTOs
{
    public record SubastaListDto(
        int Id,
        string Titulo,
        decimal PrecioBase,
        decimal PrecioActual,
        EstadoSubasta Estado,
        DateTime FechaFin,
        string CategoriaNombre,
        string VendedorNombre,
        int TotalPujas
    );
}
