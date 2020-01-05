using Microsoft.AspNet.SignalR.Client;
using SettlementSimulation.Host.Common.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using SettlementSimulation.Host.Common.Models.Dtos;

namespace SettlementSimulation.Client
{
    class Program
    {
        static void Main()
        {
            ////uncomment to get all types of supported buildings
            GetSupportedBuildings();
            Thread.Sleep(500);
            GetSupportedRoads();
            Thread.Sleep(500);
            GetTerrains();
            Thread.Sleep(500);

            RunSimulation(new RunSimulationRequest()
            {
                MinHeight = 145,
                MaxHeight = 170,
                MaxIterations = 4000,
                BreakpointStep = 5,
                HeightMap = new BitmapDto()
                {
                    Path = @"C:\Users\adams\Desktop\SS.Data\hm.png"
                }
            });

            Console.WriteLine("\n\tRunning simulation...");
            Console.ReadLine();
        }

        static void GetSupportedBuildings()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<string[]>("OnGetSupportedBuildingsResponse", response =>
                    {
                        Console.WriteLine("\nSupported buildings:");
                        response.ToList().ForEach(Console.WriteLine);
                    });
                proxy.Invoke("GetSupportedBuildings");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void GetSupportedRoads()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<string[]>("OnGetSupportedRoadsResponse", response =>
                {
                    Console.WriteLine("\nSupported roads:");
                    response.ToList().ForEach(Console.WriteLine);
                });
                proxy.Invoke("GetSupportedRoads");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void GetTerrains()
        {
            var url = ConfigurationManager.AppSettings["SettlementSimulationUrl"];
            var conn = new HubConnection(url);
            var proxy = conn.CreateHubProxy("notificationHub");

            try
            {
                conn.Start().Wait();
                proxy.On<TerrainDto[]>("OnGetTerrainsResponse", response =>
                {
                    Console.WriteLine("\nAll terrains:");
                    response.ToList().ForEach(Console.WriteLine);
                });
                proxy.Invoke("GetTerrains");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void RunSimulation(RunSimulationRequest request)
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
                proxy.On<string>("OnException", Console.WriteLine);
                proxy.Invoke("RunSimulation", request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
