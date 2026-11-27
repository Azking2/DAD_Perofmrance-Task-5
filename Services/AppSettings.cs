using System.Text.Json;

namespace HubLink.Services;

/// <summary>
/// Settings read from appsettings.json, so the address and branch can be
/// changed without touching the code.
/// </summary>
public class AppSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:5080";
    public string BranchCode { get; set; } = "BR-001";
    public string RequestPath { get; set; } = "/api/shipments";
    public string ExtraQuery { get; set; } = "";   // e.g. mode=delay (test endpoint only)
    public int TimeoutSeconds { get; set; } = 8;
    public string? LoadError { get; set; }

    public static AppSettings Load()
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), options);
                if (loaded != null)
                {
                    if (loaded.TimeoutSeconds <= 0) loaded.TimeoutSeconds = 8;
                    return loaded;
                }
            }
        }
        catch (Exception)
        {
            // If the file is unreadable, fall back to the default values.
        }
        return new AppSettings
        {
            LoadError = "Could not load appsettings.json. Check that it exists and is valid JSON, then restart the app."
        };
    }

    /// <summary>Builds the complete request URL from the settings.</summary>
    public Uri BuildUri()
    {
        string url = BaseUrl.TrimEnd('/') + "/" + RequestPath.TrimStart('/')
                     + "?branchCode=" + Uri.EscapeDataString(BranchCode);

        if (!string.IsNullOrWhiteSpace(ExtraQuery))
            url += "&" + ExtraQuery.TrimStart('&', '?');

        return new Uri(url);
    }
}
