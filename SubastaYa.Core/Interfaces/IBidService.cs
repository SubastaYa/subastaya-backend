using System;
using System.Threading.Tasks;

namespace SubastaYa.Core.Interfaces
{
    public interface IBidService
    {
        Task PlaceBidAsync(int auctionId, int buyerId, decimal amount);
    }
}

namespace SubastaYa.Core.Exceptions
{
    public class SaldoInsuficienteException : Exception
    {
        public SaldoInsuficienteException(string message) : base(message) { }
    }
}
