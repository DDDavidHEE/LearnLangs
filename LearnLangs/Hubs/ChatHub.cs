using Microsoft.AspNetCore.SignalR;

namespace LearnLangs.Hubs
{
    public class ChatHub : Hub
    {
        public Task Join(string sessionId)
            => Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        public override Task OnDisconnectedAsync(Exception? ex)
        {
            // Optional: remove from groups if you track that.
            return base.OnDisconnectedAsync(ex);
        }
    }
}
