using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.DTOs
{
    public record PujaResumenDto(
        int Id,
        decimal Monto,
        DateTime FechaHora,
        string CompradorNombre
    );
}
