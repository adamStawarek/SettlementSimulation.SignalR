using Microsoft.AspNet.SignalR;
using SettlementSimulation.AreaGenerator;
using SettlementSimulation.Engine;
using SettlementSimulation.Host.Common.Models;
using SettlementSimulation.Host.Common.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SettlementSimulation.AreaGenerator.Helpers;
using SettlementSimulation.AreaGenerator.Models;
using SettlementSimulation.AreaGenerator.Models.Terrains;
using SettlementSimulation.Engine.Enumerators;
using SettlementSimulation.Engine.Models;
using SettlementSimulation.Engine.Models.Buildings;
using SettlementSimulation.Host.Common.Enumerators;
using Direction = SettlementSimulation.Host.Common.Enumerators.Direction;
using System.IO;
using SettlementSimulation.Engine.Interfaces;
using Material = SettlementSimulation.Host.Common.Enumerators.Material;

namespace SettlementSimulation.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public void GetSupportedBuildings()
        {
            var types = Assembly.Load("SettlementSimulation.Engine")
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Building)))
                .Select(t => t.Name);

            Clients.All.OnGetSupportedBuildingsResponse(types);
        }

        public void GetSupportedRoads()
        {
            var types = Enum.GetValues(typeof(RoadType))
                .Cast<RoadType>()
                .Select(t => t.ToString());
            Clients.All.OnGetSupportedRoadsResponse(types);
        }

        public void GetTerrains(BitmapDto model)
        {
            var heightMap = new Bitmap(model.Path);
            TerrainHelper.SetTerrains(BitmapToPixelArray(heightMap));

            var terrainHelper = new TerrainHelper();

            var terrains = terrainHelper.GetAllTerrains()
                .Select(t => new TerrainDto()
                {
                    Type = t.GetType().Name,
                    UpperHeightBound = t.UpperBound
                });
            Clients.All.OnGetTerrainsResponse(terrains);
        }

        public async Task RunSimulation(RunSimulationRequest request)
        {
            File.Delete("logs.txt");
            Console.WriteLine($"Client Id: {Context.ConnectionId} " +
                              $"Time Called: {DateTime.UtcNow:D}");

            try
            {
                var heightMap = new Bitmap(request.HeightMap.Path);

                Console.WriteLine("Start processing heightMap..");
                var settlementInfo = await new SettlementBuilder()
                    .WithHeightMap(this.BitmapToPixelArray(heightMap))
                    .BuildAsync();

                Console.WriteLine("Finished processing heightMap.");

                var generator = new StructureGeneratorBuilder()
                    .WithMaxIterations(request.MaxIterations)
                    .WithBreakpointStep(request.BreakpointStep)
                    .WithFields(settlementInfo.Fields)
                    .WithMainRoad(settlementInfo.MainRoad)
                    .Build();

                Console.WriteLine("Start running simulation..");

                generator.Initialized += OnSettlementStateUpdate;
                generator.Breakpoint += OnSettlementStateUpdate;
                generator.NextEpoch += OnSettlementStateUpdate;
                generator.Finished += OnSettlementStateUpdate;
                generator.Finished += OnFinished;

                await generator.Start();
            }
            catch (Exception e)
            {
                var formattedException = $"Server exception at {nameof(NotificationHub)}.{nameof(RunSimulation)}:" +
                                         $"\nMessage: {e.Message}," +
                                         $"\nInner exception: {e.InnerException}," +
                                         $"\nStackTrace: {e.StackTrace}";
                Console.WriteLine(formattedException);
                Clients.All.onException(formattedException);
                throw;
            }
        }

        private void OnFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Simulation finished");
            Clients.All.onFinished($"Simulation finished at {DateTime.UtcNow:G}");
        }

        private void OnSettlementStateUpdate(object sender, EventArgs e)
        {
            var settlementState = ((StructureGenerator)sender).SettlementState;
            Console.WriteLine($"Breakpoint: {settlementState.CurrentIteration}");

            var buildingsDtos = new List<BuildingDto>();
            var roadDtos = new List<RoadDto>();
            var updatedBuildingsDtos = new List<Tuple<BuildingDto, BuildingDto>>();
            var updatedRoadDtos = new List<Tuple<RoadDto, RoadDto>>();
            var removedBuildingsDtos = new List<BuildingDto>();

            var settlementUpdate = settlementState.LastSettlementUpdate;

            settlementUpdate.NewRoads.ForEach(road =>
            {
                roadDtos.Add(ConvertToRoadDto(road));
                road.Buildings.ForEach(b =>
                    buildingsDtos.Add(this.ConvertToBuildingDto(b)));
            });
            settlementUpdate.NewBuildings.ForEach(b =>
            {
                buildingsDtos.Add(this.ConvertToBuildingDto(b));
            });
            settlementUpdate.UpdatedBuildings.ForEach(tuple =>
            {
                var oldBDto = this.ConvertToBuildingDto(tuple.oldBuilding);
                var newBDto = this.ConvertToBuildingDto(tuple.newBuilding);
                updatedBuildingsDtos.Add(new Tuple<BuildingDto, BuildingDto>(oldBDto, newBDto));
            });
            settlementUpdate.UpdatedRoads.ForEach(tuple =>
            {
                var oldRDto = ConvertToRoadDto(tuple.oldRoad);
                var newRDto = ConvertToRoadDto(tuple.newRoad);
                updatedRoadDtos.Add(new Tuple<RoadDto, RoadDto>(oldRDto, newRDto));
            });

            settlementUpdate.FloodMutationResult?.RemovedBuildings.ForEach(b =>
            {
                removedBuildingsDtos.Add(ConvertToBuildingDto(b));
            });

            settlementUpdate.EarthquakeMutationResult?.RemovedBuildings.ForEach(b =>
            {
                removedBuildingsDtos.Add(ConvertToBuildingDto(b));
            });

            var response = new RunSimulationResponse()
            {
                CurrentEpoch = (int)settlementState.CurrentEpoch,
                CurrentGeneration = settlementState.CurrentIteration,
                MainRoad = new RoadDto()
                {
                    Locations = settlementState.MainRoad.Segments
                        .Select(p => new LocationDto(p.Position.X, p.Position.Y))
                        .ToArray()
                },
                Buildings = settlementState.Roads
                    .SelectMany(r => r.Buildings)
                    .Select(ConvertToBuildingDto)
                    .ToArray(),
                Roads = settlementState.Roads
                    .Select(ConvertToRoadDto)
                    .ToArray(),
                LastGeneratedBuildings = buildingsDtos.ToArray(),
                LastGeneratedRoads = roadDtos.ToArray(),
                LastUpdatedBuildings = updatedBuildingsDtos.ToArray(),
                LastUpdatedRoads = updatedRoadDtos.ToArray(),
                LastRemovedBuildings = removedBuildingsDtos.ToArray()
            };
            File.AppendAllText("logs.txt", response.ToString());
            Clients.All.onSettlementStateUpdate(response);
        }

        private BuildingDto ConvertToBuildingDto(IBuilding building)
        {
            return new BuildingDto()
            {
                Type = building.GetType().Name,
                Location = new LocationDto(building.Position.X, building.Position.Y),
                Direction = (Direction)Enum.Parse(typeof(Direction), building.Direction.ToString()),
                Material = (Material)Enum.Parse(typeof(Material), building.Material.Value.ToString())
            };
        }

        private RoadDto ConvertToRoadDto(IRoad road)
        {
            return new RoadDto()
            {
                Type = road.Type.ToString(),
                Locations = road.Segments
                    .Select(p => new LocationDto(p.Position.X, p.Position.Y))
                    .ToArray()
            };
        }

        private Pixel[,] BitmapToPixelArray(Bitmap bitmap)
        {
            var pixels = new Pixel[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    pixels[i, j] = new Pixel(color.R, color.G, color.B);
                }
            }

            return pixels;
        }
    }
}
