using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Nnn.ApplicationCore.Services
{
    public class ChatHub : Hub
    {
        public async Task SendToChannel(string name, string message)
        {
           await  Clients.All.SendAsync("SendToChannel", name, message);
        }
    } 
}