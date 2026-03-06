using Microsoft.AspNetCore.SignalR;

namespace MarketingAssistant.Api.Hubs;

public class DashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
