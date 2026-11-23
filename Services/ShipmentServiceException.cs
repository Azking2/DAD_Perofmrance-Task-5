namespace HubLink.Services;

/// <summary>Thrown when shipments cannot be retrieved. The message is safe to show to the user.</summary>
public class ShipmentServiceException : Exception
{
    public ShipmentServiceException(string message) : base(message) { }
}
