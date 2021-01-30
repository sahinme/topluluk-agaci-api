using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Nnn.ApplicationCore.Services.RealNotify
{
    public class UserIdProvider:IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User.Identity.Name;
        }
    }
}