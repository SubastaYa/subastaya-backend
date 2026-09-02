using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Core.DTOs.Auth
{
    public record AuthResponseDto(string Token, string Email);
}
