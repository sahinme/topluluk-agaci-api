using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace Microsoft.Nnn.Web
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                        .Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
