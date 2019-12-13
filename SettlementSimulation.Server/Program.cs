using Microsoft.Owin.Hosting;
using System;
using System.Configuration;

namespace SettlementSimulation.Server
{
    class Program
    {
        static void Main()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"[{DateTime.UtcNow:D}] Server started at {url}");
                Console.ReadKey();
            }
        }
    }
}
