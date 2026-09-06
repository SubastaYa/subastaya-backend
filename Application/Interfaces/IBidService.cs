namespace Application.Interfaces
{
    public interface IBidService
    {
        Task PlaceBidAsync(int auctionId, int buyerId, decimal amount);
    }
}

