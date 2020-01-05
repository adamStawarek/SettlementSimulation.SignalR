namespace SettlementSimulation.Host.Common.Models.Dtos
{
    public class TerrainDto
    {
        public string Type { get; set; }
        public byte UpperHeightBound { get; set; }
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type} " +
                   $"{nameof(UpperHeightBound)}: {UpperHeightBound}";
        }
    }
}