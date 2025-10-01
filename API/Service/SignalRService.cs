using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace API.Service;

/// <summary>
/// The hub for our ticketing system
/// </summary>
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
    private const string AdminUserId = "4f8a2997-3e89-4e19-826b-062391224f58";

    /// <summary>
    /// Adds a user to the ConnectedUsers and creates a value pair userId -> ConnectionId. Also sends a welcome message,
    /// which will not be logged in the DB.
    /// </summary>
    /// <param name="userId">A users ID</param>
    public async Task JoinUserChat(string userId)
    {
        ConnectedUsers.AddOrUpdate(userId, Context.ConnectionId, (_, _) => Context.ConnectionId);
        await SendWelcomeMessage("System", $"{userId} has joined the chat");
    }

    private async Task SendWelcomeMessage(string senderUserId, string message)
    {
        if (ConnectedUsers.TryGetValue(AdminUserId, out var adminConnId))
        {
            await Clients.Clients(adminConnId, Context.ConnectionId)
                .SendAsync("ReceiveUserJoinedMessage", senderUserId, senderUserId, message);
        }
        else
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("ReceiveUserJoinedMessage", "System", "System", "Admin is not connected");
        }
    }
    
    /// <summary>
    /// A user sends a message to the hardcoded Admin and themselves. If the admin is not found in the ConnectedUsers,
    /// so not logged on, the user will instead be given a message saying the admin is not connected.
    /// </summary>
    /// <param name="senderUserId">The user ID of the user that wants to send a message</param>
    /// <param name="message">The message</param>
    public async Task SendMessageToAdmin(string senderUserId, string message)
    {
        if (ConnectedUsers.TryGetValue(AdminUserId, out var adminConnId))
        {
            await Clients.Clients(adminConnId, Context.ConnectionId)
                .SendAsync("ReceiveMessage", AdminUserId, senderUserId, message);
        }
        else
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("ReceiveMessage", "System", "System", "Admin is not connected");
        }
    }

    /// <summary>
    /// An admin replies to a user and sens it to the targeted user ID and admin. If the user us not found in
    /// ConnectedUser, we instead send a message to the admin saying that the targeted user was not found
    /// </summary>
    /// <param name="targetUserId"></param>
    /// <param name="message"></param>
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

    /// <summary>
    /// Removes the user from the ConnectedUsers pool
    /// </summary>
    /// <param name="exception">I dont know what the hell this is for</param>
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