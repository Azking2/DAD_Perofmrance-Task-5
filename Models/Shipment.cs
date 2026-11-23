namespace HubLink.Models;

/// <summary>One shipment record returned by the HubLink API.</summary>
public class Shipment
{
    public string TrackingNumber { get; set; } = "";
    public string BranchCode { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset LastUpdatedUtc { get; set; }

    // Text shown in the table (UTC, so it matches the CSV file)
    public string LastUpdatedDisplay =>
        LastUpdatedUtc.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
}
