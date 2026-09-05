using System;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBidService
    {
        Task PlaceBidAsync(int auctionId, int buyerId, decimal amount);
    }
}

namespace Application.Exceptions
{
    public class SaldoInsuficienteException : Exception
    {
        public SaldoInsuficienteException(string message) : base(message) { }
    }
}
