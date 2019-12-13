using System.Linq;

namespace SettlementSimulation.Host.Common.Models.Dtos
{
    public class RoadDto
    {
        public string Type { get; set; }
        public LocationDto[] Locations { get; set; }
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type} " +
                   $"{nameof(Locations)}: \n" +
                   $"{Locations.Aggregate("", (l1, l2) => l1 + "\n\t " + l2, r => r.ToString())} ";
        }
    }
}