# HubLink – Shipment Viewer (Performance Task 5)

HubLink is an Avalonia (C# / .NET 8) desktop app that connects to the HubLink shipment REST API.
It retrieves shipments for one branch, shows them in a table, filters by status, shows a summary,
and exports the displayed shipments to a CSV file.

**Team members:** James A. Dales and John Paul A. Canarejo   **Section:** BSIT-3C

## Requirements
- .NET 8 SDK
- The HubLink service running (see below)

## 1. Start the API
1. Extract the whole `HubLink-PT5-Local-Server-Windows-x64.zip`.
2. Double-click `Start-HubLink.cmd` and keep the window open.
   (If port 5080 is busy, use `Start-HubLink-5081.cmd` and change the port in `appsettings.json`.)

## 2. Configure the app
Edit `appsettings.json` (no code changes needed):

| Setting | Meaning | Default |
|---|---|---|
| `BaseUrl` | Address of the API | `http://127.0.0.1:5080` |
| `BranchCode` | Branch to retrieve (BR-001, BR-002, BR-003) | `BR-001` |
| `RequestPath` | Endpoint path | `/api/shipments` |
| `ExtraQuery` | Extra query text, used only for test modes | _(empty)_ |
| `TimeoutSeconds` | Client timeout | `8` |

## 3. Restore and run
```
dotnet restore
dotnet run
```
The app loads the shipments automatically when the window opens.

## How to use
- **Status filter** – choose All / Pending / InTransit / Delivered / Exception. No new HTTP request is made.
- **Refresh** – gets the data from the API again. The button is disabled while loading.
- **Retry** – appears after a failure and repeats the request.
- **Export CSV** – saves the shipments currently shown in the table (respects the filter).
- The **summary** always counts all retrieved shipments of the branch, regardless of the filter.

## Testing error cases (test endpoint)
Change these settings, restart the app, then **set them back** to the normal values afterwards:

| Case | `RequestPath` | `ExtraQuery` |
|---|---|---|
| Normal | `/api/shipments` | _(empty)_ |
| Empty result | `/api/test/shipments` | `mode=empty` |
| HTTP 400 | `/api/test/shipments` | `mode=bad-request` |
| HTTP 500 | `/api/test/shipments` | `mode=server-error` |
| Invalid JSON | `/api/test/shipments` | `mode=invalid-json` |
| Timeout | `/api/test/shipments` | `mode=delay` (service waits 10 s, app times out at 8 s) |
| Unreachable | normal settings, but close the service window | |

## Project structure
- `Models/` – `Shipment`, `ShipmentResponse` (JSON data classes)
- `Services/` – `ShipmentService` (HttpClient + JSON + validation), `CsvExporter`, `AppSettings`
- `ViewModels/` – `MainWindowViewModel` (filter, summary, loading, errors, export logic), commands
- `Views/` – `MainWindow.axaml` (XAML only; code-behind just opens the Save dialog)

## Notes
- The app only reads data (GET). It does not create, edit or delete shipments.
- AI assistance: [AI tool John Paul used, if any] was used for [what it helped with]. Claude was also used to review the code, test the app logic against a stand-in server, suggest fixes for settings-file error handling and the column width, guide our manual testing, and draft the verification checklist.
