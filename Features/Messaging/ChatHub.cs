using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using HostelManagementSystemApi.Persistence;
using HostelManagementSystemApi.Domain;
using System.Threading.Tasks;
using System;

namespace HostelManagementSystemApi.Features.Messaging
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private static readonly ConcurrentDictionary<int, string> UserConnections = new ConcurrentDictionary<int, string>();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int recipientUserId, string message)
        {
            var senderUserIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(senderUserIdString, out var senderUserId))
            {
                // This should not happen for an authenticated user
                return;
            }

            // Save the message to the database
            var chatMessage = new ChatMessage
            {
                SenderId = senderUserId,
                RecipientId = recipientUserId,
                Content = message,
                Timestamp = DateTime.UtcNow
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send the message to the recipient if they are connected
            if (UserConnections.TryGetValue(recipientUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderUserId, message);
            }
        }

        public override Task OnConnectedAsync()
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out var userId))
            {
                UserConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out var userId))
            {
                UserConnections.TryRemove(userId, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}

