namespace HubLink.Models;

/// <summary>The outer JSON object: { "branchCode": "...", "shipments": [ ... ] }</summary>
public class ShipmentResponse
{
    public string? BranchCode { get; set; }
    public List<Shipment>? Shipments { get; set; }
}
