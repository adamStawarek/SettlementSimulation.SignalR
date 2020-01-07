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
                   $"Start: {Locations.First()}, End: {Locations.Last()}";
        }
    }
}