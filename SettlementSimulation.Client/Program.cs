using Microsoft.AspNet.SignalR.Client;
using SettlementSimulation.Host.Common.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SettlementSimulation.Client
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("\tSupported buildings:");
            var buildings = await GetSupportedBuildings();
            buildings.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\tSupported roads:");
            var roads = await GetSupportedRoads();
            roads.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n\tPress enter to start signalR services");
            Console.ReadLine();
            RunSignalR(new RunSimulationRequest()
            {
                MinHeight = 100,
                MaxHeight = 180,
                MaxIterations = 100,
                Breakpoints = new[] { 20, 40, 60 },
                //ColorMap = new Host.Common.Models.Dtos.BitmapDto()
                //{
                //  Path = @"C:\Users\adams\Desktop\SS.Data\colourmap.png"
                //},
                HeightMap = new Host.Common.Models.Dtos.BitmapDto()
                {
                    Path = @"C:\Users\adams\Desktop\SS.Data\heightmap.png"
                }
            });

            Console.WriteLine("\n\tRunning simulation...");
            Console.ReadLine();
        }

        static async Task<string[]> GetSupportedBuildings()
        {
            var client = new HttpClient();
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var response = await client.GetAsync($"{url}/api/Simulation/GetBuildings");

            return response.Content.ReadAsAsync<string[]>().Result;
        }

        static async Task<string[]> GetSupportedRoads()
        {
            var client = new HttpClient();
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var response = await client.GetAsync($"{url}/api/Simulation/GetRoads");

            return response.Content.ReadAsAsync<string[]>().Result;
        }

        static void RunSignalR(RunSimulationRequest request)
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<RunSimulationResponse>("OnSettlementStateUpdate", response =>
                {
                    Console.Clear();
                    Console.WriteLine($"Response at {DateTime.UtcNow:G}");
                    Console.WriteLine(response);
                });
                proxy.On<string>("OnFinished", Console.WriteLine);
                proxy.Invoke("RunSimulation", request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
