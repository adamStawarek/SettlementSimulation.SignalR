﻿using Microsoft.AspNet.SignalR;
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
using SettlementSimulation.Engine.Enumerators;
using SettlementSimulation.Engine.Models;
using SettlementSimulation.Engine.Models.Buildings;

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

        public void GetTerrains()
        {
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
            Console.WriteLine($"Client Id: {Context.ConnectionId} " +
                              $"Time Called: {DateTime.UtcNow:D}");


            try
            {
                var heightMap = new Bitmap(request.HeightMap.Path);

                var settlementBuilder = new SettlementBuilder()
                    .WithHeightMap(this.BitmapToPixelArray(heightMap))
                    .WithHeightRange(request.MinHeight, request.MaxHeight);

                var settlementInfo = await settlementBuilder.BuildAsync();

                var generator = new StructureGeneratorBuilder()
                    .WithMaxIterations(request.MaxIterations)
                    .WithBreakpointStep(request.BreakpointStep)
                    .WithFields(settlementInfo.Fields)
                    .WithMainRoad(settlementInfo.MainRoad)
                    .Build();

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
            Clients.All.onFinished($"Simulation finished at {DateTime.UtcNow:G}");
        }

        private void OnSettlementStateUpdate(object sender, EventArgs e)
        {
            var settlementState = ((StructureGenerator)sender).SettlementState;

            var lastCreatedStructures = settlementState.LastCreatedStructures.ToList();
            var buildingsDtos = new List<BuildingDto>();
            var roadDtos = new List<RoadDto>();
            if (lastCreatedStructures.Any())
            {
                lastCreatedStructures.ForEach(s =>
                {
                    switch (s)
                    {
                        case Road road:
                            roadDtos.Add(new RoadDto()
                            {
                                Type = road.Type.ToString(),
                                Locations = road.Segments
                                   .Select(p => new LocationDto(p.Position.X, p.Position.Y))
                                   .ToArray()
                            });

                            road.Buildings.ForEach(b =>
                                buildingsDtos.Add(new BuildingDto()
                                {
                                    Type = b.GetType().Name,
                                    Location = new LocationDto(b.Position.X, b.Position.Y)
                                }));
                            break;
                        case Building building:
                            buildingsDtos.Add(new BuildingDto()
                            {
                                Type = building.GetType().Name,
                                Location = new LocationDto(building.Position.X, building.Position.Y)
                            });
                            break;
                    }
                });

            }

            Clients.All.onSettlementStateUpdate(new RunSimulationResponse()
            {
                CurrentEpoch = (int)settlementState.CurrentEpoch,
                CurrentGeneration = settlementState.CurrentGeneration,
                Buildings = settlementState.Roads
                    .SelectMany(r => r.Buildings)
                    .Select(b => new BuildingDto()
                    {
                        Type = b.GetType().Name,
                        Location = new LocationDto(b.Position.X, b.Position.Y)
                    })
                    .ToArray(),
                Roads = settlementState.Roads
                    .Select(r => new RoadDto()
                    {
                        Type = r.GetType().Name,
                        Locations = r.Segments
                            .Select(p => new LocationDto(p.Position.X, p.Position.Y))
                            .ToArray()
                    })
                    .ToArray(),
                LastGeneratedBuildings = buildingsDtos.ToArray(),
                LastGeneratedRoads = roadDtos.ToArray()
            });
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
