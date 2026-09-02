using SubastaYa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.Interfaces
{
    public interface IJwtProvider
    {
        string GenerarToken(Usuario usuario);
    }
}
