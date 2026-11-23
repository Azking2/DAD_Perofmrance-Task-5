using System.Globalization;
using System.Text;
using HubLink.Models;

namespace HubLink.Services;

/// <summary>Creates CSV text from shipments and writes it to a file.</summary>
public static class CsvExporter
{
    public static string BuildCsv(IEnumerable<Shipment> shipments)
    {
        var sb = new StringBuilder();
        sb.Append("TrackingNumber,Origin,Destination,Status,LastUpdatedUtc\r\n");

        foreach (var s in shipments)
        {
            string time = s.LastUpdatedUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

            sb.Append(Escape(s.TrackingNumber)).Append(',')
              .Append(Escape(s.Origin)).Append(',')
              .Append(Escape(s.Destination)).Append(',')
              .Append(Escape(s.Status)).Append(',')
              .Append(Escape(time)).Append("\r\n");
        }

        return sb.ToString();
    }

    public static async Task ExportAsync(string path, IEnumerable<Shipment> shipments)
    {
        string csv = BuildCsv(shipments);
        await File.WriteAllTextAsync(path, csv, new UTF8Encoding(false));
    }

    // Wrap a value in quotes if it contains a comma, quote or line break.
    // Quotes inside the value are doubled ("" ).
    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }
}
