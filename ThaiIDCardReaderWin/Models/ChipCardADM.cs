namespace ThaiIDCardReader.Models;

/// <summary>
/// Represents chip card administration information
/// </summary>
public class ChipCardADM
{
    public string Version { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Authorize { get; set; } = string.Empty;
    public string LaserNumber { get; set; } = string.Empty;
}
