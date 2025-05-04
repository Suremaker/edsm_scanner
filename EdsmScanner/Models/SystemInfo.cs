namespace EdsmScanner.Models;

internal class SystemInfo
{
    public static SystemInfo None { get; } = new();
    public string Allegiance { get; init; } = string.Empty;
    public string Government { get; init; } = string.Empty;
    public string Faction { get; init; } = string.Empty;
    public string FactionState { get; init; } = string.Empty;
    public long Population { get; init; }
    public string Security { get; init; } = string.Empty;
    public string Economy { get; init; } = string.Empty;
    public string SecondEconomy { get; init; } = string.Empty;
    public string Reserve { get; init; } = string.Empty;
}