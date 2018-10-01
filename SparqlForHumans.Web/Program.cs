using System.Linq;
using Microsoft.AspNetCore.Blazor.Hosting;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
