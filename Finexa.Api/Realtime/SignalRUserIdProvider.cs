using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Finexa.Api.Realtime
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? connection.User?.FindFirstValue("sub");
        }
    }
}