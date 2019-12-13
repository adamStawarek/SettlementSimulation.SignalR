using Microsoft.AspNet.SignalR;
using SettlementSimulation.AreaGenerator;
using SettlementSimulation.Engine;
using SettlementSimulation.Engine.Models.Buildings;
using SettlementSimulation.Engine.Models.Roads;
using SettlementSimulation.Host.Common.Models;
using SettlementSimulation.Host.Common.Models.Dtos;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SettlementSimulation.Server.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task RunSimulation(RunSimulationRequest request)
        {
            Console.WriteLine($"Client Id: {Context.ConnectionId} " +
                              $"Time Called: {DateTime.UtcNow:D}");

            var colorMap = new Bitmap(request.ColorMap.Path);
            var heightMap = new Bitmap(request.HeightMap.Path);
            //var colorMap = this.CopyDataToBitmap(request.ColorMap);
            //var heightMap = this.CopyDataToBitmap(request.HeightMap);

            var settlementInfo = await new SettlementBuilder()
                .WithColorMap(colorMap)
                .WithHeightMap(heightMap)
                .WithHeightRange(request.MinHeight, request.MaxHeight)
                .BuildAsync();

            var generator = new StructureGeneratorBuilder()
                .WithMaxIterations(request.MaxIterations)
                .WithBreakpoints(request.Breakpoints)
                .WithFields(settlementInfo.Fields)
                .WithMainRoad(settlementInfo.MainRoad)
                .Build();

            generator.Breakpoint += OnSettlementStateUpdate;
            generator.NextEpoch += OnSettlementStateUpdate;
            generator.Finished += OnSettlementStateUpdate;
            generator.Finished += OnFinished;
            
            generator.Start();
        }

        private void OnFinished(object sender, EventArgs e)
        {
            Clients.All.onFinished($"Simulation finished at {DateTime.UtcNow:G}");
        }

        private void OnSettlementStateUpdate(object sender, EventArgs e)
        {
            var settlementState = ((StructureGenerator)sender).SettlementState;
            
            Clients.All.onSettlementStateUpdate(new RunSimulationResponse()
            {
                CurrentEpoch = (int)settlementState.CurrentEpoch,
                CurrentGeneration = settlementState.CurrentGeneration,
                Buildings = settlementState.Structures
                    .Where(b => b is Building)
                    .Cast<Building>()
                    .Select(b => new BuildingDto()
                    {
                        Type = b.GetType().Name,
                        Location = new LocationDto(b.Location.X, b.Location.Y)
                    })
                    .ToArray(),
                Roads = settlementState.Structures
                    .Where(r => r is Road)
                    .Cast<Road>()
                    .Select(r => new RoadDto()
                    {
                        Type = r.GetType().Name,
                        Locations = r.Points
                            .Select(p => new LocationDto(p.X, p.Y))
                            .ToArray()
                    })
                    .ToArray(),
            });
        }

        public Bitmap CopyDataToBitmap(int width, int height, byte[] data)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
