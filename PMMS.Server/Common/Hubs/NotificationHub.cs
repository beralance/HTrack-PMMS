using Microsoft.AspNetCore.SignalR;

namespace PMMS.Server.Common.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var provinceClaims = user.FindAll("assigned_province");

            foreach (var claim in provinceClaims)
            {
                if (int.TryParse(claim.Value, out var provinceId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Province_{provinceId}");
                }
            }
        }

        await base.OnConnectedAsync();
    }
}