namespace EdsmScanner.Models
{
    internal class SystemRef
    {
        public decimal Distance { get; init; }
        public int? BodyCount { get; init; }
        public string Name { get; init; } = string.Empty;
        public CoordF Coords { get; init; }
        public SystemInfo Information { get; init; } = SystemInfo.None;
        public override string ToString() => $"{Name} [{Distance}ly] ({BodyCount?.ToString() ?? "?"} bodies)";
    }
}