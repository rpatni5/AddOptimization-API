using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AddOptimization.Services.NotificationHelpers
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, List<string>> UserConnections = new Dictionary<string, List<string>>();

        public async Task SendMessageToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong");
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("id")?.Value;
            if (userId != null)
            {
                if (!UserConnections.ContainsKey(userId))
                {
                    UserConnections[userId] = new List<string>();
                }
                UserConnections[userId].Add(Context.ConnectionId);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst("id")?.Value;
            if (userId != null && UserConnections.ContainsKey(userId))
            {
                UserConnections[userId].Remove(Context.ConnectionId);
                if (UserConnections[userId].Count == 0)
                {
                    UserConnections.Remove(userId);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        internal static List<string> GetConnections(string userId)
        {
            List<string> connectionIds = new List<string>();
            if (UserConnections.ContainsKey(userId))
            {
                connectionIds = UserConnections[userId];
            }
            return connectionIds;
        }
    }
}
