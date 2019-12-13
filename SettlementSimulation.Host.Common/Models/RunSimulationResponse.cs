using System.Linq;
using SettlementSimulation.Host.Common.Models.Dtos;

namespace SettlementSimulation.Host.Common.Models
{
    public class RunSimulationResponse
    {
        public int CurrentEpoch { get; set; }
        public int CurrentGeneration { get; set; }
        public BuildingDto[] Buildings { get; set; }
        public RoadDto[] Roads { get; set; }

        public override string ToString()
        {
            return $"{nameof(CurrentEpoch)}: {CurrentEpoch} \n" +
                   $"{nameof(CurrentGeneration)}: {CurrentGeneration} \n" +
                   $"{nameof(Buildings)}: \n" +
                   $"{Buildings.Aggregate("", (b1, b2) => b1 + "\n\t " + b2, b => b.ToString())} \n" +
                   $"{nameof(Roads)}: \n" +
                   $"{Roads.Aggregate("", (r1, r2) => r1 + "\n\t " + r2, r => r.ToString())}";
        }
    }
}