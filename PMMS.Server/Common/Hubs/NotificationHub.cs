using Microsoft.AspNetCore.SignalR;

namespace PMMS.Server.Common.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // 1. Read the user principal safely out of the connection context
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // 2. Extract all assigned province IDs from their identity claims token
            var provinceClaims = user.FindAll("assigned_province");

            foreach (var claim in provinceClaims)
            {
                if (int.TryParse(claim.Value, out var provinceId))
                {
                    // 3. Drop this connection into a province-specific room
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Province_{provinceId}");
                }
            }
        }

        await base.OnConnectedAsync();
    }
}