using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Finexa.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}