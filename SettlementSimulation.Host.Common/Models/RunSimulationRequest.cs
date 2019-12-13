using SettlementSimulation.Host.Common.Models.Dtos;

namespace SettlementSimulation.Host.Common.Models
{
    public class RunSimulationRequest
    {
        public BitmapDto ColorMap { get; set; }
        public BitmapDto HeightMap { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }
        public int MaxIterations { get; set; }
        public int[] Breakpoints { get; set; }
    }
}
