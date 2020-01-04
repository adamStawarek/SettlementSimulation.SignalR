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
        public dynamic LastGeneratedStructure { get; set; }

        public override string ToString()
        {
            return $"{nameof(CurrentEpoch)}: {CurrentEpoch} \n" +
                   $"{nameof(CurrentGeneration)}: {CurrentGeneration} \n" +
                   $"{nameof(Buildings)}: {Buildings.Count()} \n" +
                   $"{nameof(Roads)}: {Roads.Count()} \n" +
                   $"{nameof(LastGeneratedStructure)}: {LastGeneratedStructure}";
        }
    }
}