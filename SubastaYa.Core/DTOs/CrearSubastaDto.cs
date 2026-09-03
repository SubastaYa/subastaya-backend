using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.DTOs
{
    public record CrearSubastaDto(
        string Titulo,
        string Descripcion,
        string UrlImagen,
        decimal PrecioBase,
        decimal IncrementoMinimo,
        DateTime FechaInicio,
        DateTime FechaFin,
        int CategoriaId
    );
}
