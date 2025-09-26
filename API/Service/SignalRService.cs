using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace API.Service;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
    private const string AdminUserId = "4f8a2997-3e89-4e19-826b-062391224f58";

    public async Task JoinUserChat(string userId)
    {
        ConnectedUsers.AddOrUpdate(userId, Context.ConnectionId, (k, v) => Context.ConnectionId);
        await SendWelcomeMessage("System", $"{userId} has joined the chat");
    }

    private async Task SendWelcomeMessage(string senderUserId, string message)
    {
        if (ConnectedUsers.TryGetValue(AdminUserId, out var adminConnId))
        {
            await Clients.Clients(adminConnId, Context.ConnectionId)
                .SendAsync("ReceiveUserJoinedMessage", adminConnId, senderUserId, message);
        }
        else
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("ReceiveUserJoinedMessage", "System", "System", "Admin is not connected");
        }
    }
    
    public async Task SendMessageToAdmin(string senderUserId, string message)
    {
        if (ConnectedUsers.TryGetValue(AdminUserId, out var adminConnId))
        {
            await Clients.Clients(adminConnId, Context.ConnectionId)
                .SendAsync("ReceiveMessage", adminConnId, senderUserId, message);
        }
        else
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("ReceiveMessage", "System", "System", "Admin is not connected");
        }
    }

    public async Task AdminReplyToUser(string targetUserId, string message)
    {
        if (Context.UserIdentifier == AdminUserId || Context.ConnectionId == ConnectedUsers[AdminUserId])
        {
            if (ConnectedUsers.TryGetValue(targetUserId, out var targetConnId))
            {
                await Clients.Clients(targetConnId, Context.ConnectionId)
                    .SendAsync("ReceiveMessageAsAdmin", AdminUserId, targetUserId, message);
            }
            else
            {
                Console.WriteLine("User not found");
                await Clients.Client(Context.ConnectionId)
                    .SendAsync("ReceiveMessage", "System", "System", $"User '{targetUserId}' not found");
            }
        }
    }

    public void PrintUsers()
    {
        foreach (var user in ConnectedUsers)
        {
            Console.WriteLine($"User: {user.Key}, ConnectionId: {user.Value}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(user.Key))
        {
            ConnectedUsers.TryRemove(user.Key, out _);
            await SendMessageToAdmin("System", $"{user.Key} has left the chat");
        }

        await base.OnDisconnectedAsync(exception);
    }
}