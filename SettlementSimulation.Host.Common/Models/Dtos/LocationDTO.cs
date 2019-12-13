namespace SettlementSimulation.Host.Common.Models.Dtos
{
    public class LocationDto
    {
        public LocationDto(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return $"({nameof(X)}: {X}, " +
                   $"{nameof(Y)}: {Y})";
        }
    }
}