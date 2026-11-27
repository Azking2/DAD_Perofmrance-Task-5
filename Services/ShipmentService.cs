using System.Net;
using System.Text.Json;
using HubLink.Models;

namespace HubLink.Services;

/// <summary>
/// Handles all communication with the HubLink REST API:
/// sends the HTTP request, checks the status code and converts the JSON into Shipment objects.
/// </summary>
public class ShipmentService : IShipmentService
{
    private static readonly string[] ValidStatuses = { "Pending", "InTransit", "Delivered", "Exception" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppSettings _settings;
    private readonly HttpClient _http;

    public ShipmentService(AppSettings settings)
    {
        _settings = settings;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds) };
    }

    public async Task<List<Shipment>> GetShipmentsAsync()
    {
        if (_settings.LoadError != null)
            throw new ShipmentServiceException(_settings.LoadError);

        // 1. Build the request address
        Uri uri;
        try
        {
            uri = _settings.BuildUri();
        }
        catch (UriFormatException)
        {
            throw new ShipmentServiceException(
                "The service address in appsettings.json is not valid. Please check BaseUrl and RequestPath.");
        }

        // 2. Send the HTTP request
        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(uri);
        }
        catch (TaskCanceledException)
        {
            throw new ShipmentServiceException(
                $"The request timed out after {_settings.TimeoutSeconds} seconds. The service may be slow or unavailable.");
        }
        catch (HttpRequestException)
        {
            throw new ShipmentServiceException(
                $"Cannot reach the service at {_settings.BaseUrl}. Make sure the HubLink service is running and the address is correct.");
        }
        catch (InvalidOperationException)
        {
            throw new ShipmentServiceException(
                "The service address in appsettings.json is not valid (it must start with http:// or https://).");
        }

        using (response)
        {
            string body = await response.Content.ReadAsStringAsync();

            // 3. Unsuccessful HTTP status is NOT a successful retrieval
            if (!response.IsSuccessStatusCode)
            {
                string detail = ReadErrorText(body);
                int code = (int)response.StatusCode;
                throw new ShipmentServiceException(
                    $"The service returned an error (HTTP {code} {response.ReasonPhrase})." +
                    (detail != "" ? " " + detail : ""));
            }

            // 4. Convert the JSON into objects
            ShipmentResponse? data;
            try
            {
                data = JsonSerializer.Deserialize<ShipmentResponse>(body, JsonOptions);
            }
            catch (JsonException)
            {
                throw new ShipmentServiceException(
                    "The service replied, but the response is not valid JSON and cannot be read.");
            }

            // 5. Check that the data really looks like shipment information
            if (data == null || data.Shipments == null || string.IsNullOrWhiteSpace(data.BranchCode))
            {
                throw new ShipmentServiceException(
                    "The service replied, but the response does not contain the expected shipment information.");
            }

            if (data.BranchCode != _settings.BranchCode)
            {
                throw new ShipmentServiceException(
                    $"The response is for branch {data.BranchCode}, but {_settings.BranchCode} was requested.");
            }

            foreach (var s in data.Shipments)
            {
                if (s == null
                    || string.IsNullOrWhiteSpace(s.TrackingNumber)
                    || string.IsNullOrWhiteSpace(s.Origin)
                    || string.IsNullOrWhiteSpace(s.Destination)
                    || s.LastUpdatedUtc == default
                    || !ValidStatuses.Contains(s.Status))
                {
                    throw new ShipmentServiceException(
                        "The service replied, but one or more shipment records are incomplete or invalid.");
                }
            }

            // An empty list is a valid, successful result (not an error)
            return data.Shipments;
        }
    }

    // Tries to read {"error":"..."} from an error response
    private static string ReadErrorText(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("error", out var err) &&
                err.ValueKind == JsonValueKind.String)
            {
                return err.GetString() ?? "";
            }
        }
        catch (JsonException)
        {
            // body was not JSON - ignore
        }
        return "";
    }
}
