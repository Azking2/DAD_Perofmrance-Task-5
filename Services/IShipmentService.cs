using HubLink.Models;

namespace HubLink.Services;

public interface IShipmentService
{
    Task<List<Shipment>> GetShipmentsAsync();
}
