using System;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Hosting;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Web
{
    public class Program
    {
        //private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static void Main(string[] args)
        {
            //Logger.Info(string.Join(",", args.Select(x=>x.ToString())));

            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
