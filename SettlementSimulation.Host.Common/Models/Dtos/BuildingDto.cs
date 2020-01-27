using SettlementSimulation.Host.Common.Enumerators;

namespace SettlementSimulation.Host.Common.Models.Dtos
{
    public class BuildingDto
    {
        public string Type { get; set; }
        public LocationDto Location { get; set; }
        public Direction Direction { get; set; }
        public Material Material { get; set; }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type} " +
                   $"{nameof(Location)}: {Location}";
        }
    }
}