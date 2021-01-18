namespace EdsmScanner
{
    internal class SystemRef
    {
        public decimal Distance { get; set; }
        public int? BodyCount { get; set; }
        public string Name { get; set; } = string.Empty;
        public CoordF Coords { get; set; }
        public override string ToString() => $"{Name} [{Distance}ly] ({BodyCount?.ToString() ?? "?"} bodies)";
    }
}