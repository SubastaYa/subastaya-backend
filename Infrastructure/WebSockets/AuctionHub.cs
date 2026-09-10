using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.WebSockets
{
    public class AuctionHub : Hub
    {
        public async Task JoinAuctionGroup(int auctionId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());

        public async Task LeaveAuctionGroup(int auctionId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
    }
}
