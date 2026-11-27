# Verification Checklist – HubLink (PT5)

Tested on November 26–27, 2026, using the provided local service and branch BR-001.

| ID | Scenario | How we checked it | Result |
|---|---|---|---|
| AC-01 | Retrieve shipments | Ran the service and app. The app displayed the 5 BR-001 shipments, consistent with the API response. | Passed |
| AC-02 | Status filters | Checked Pending (1 row), InTransit (2), Delivered (1), Exception (1), and All (5). | Passed |
| AC-03 | Summary | Summary stayed at Total 5, Pending 1, InTransit 2, Delivered 1, Exception 1 across filters. | Passed |
| AC-04 | Refresh and loading | Refreshed successfully using the normal endpoint. Using mode=delay, saw the progress bar and disabled Refresh and Export buttons, and dragged the window during loading. | Passed |
| AC-05 | Service errors and retry | Tested stopped service, timeout (mode=delay), and HTTP 500 (mode=server-error). After the stopped-service test, restarted the service and Retry loaded 5 shipments and cleared the warnings. Old rows were labeled during the failure. | Passed |
| AC-06 | Empty and invalid response | mode=empty showed zero shipments without an error. mode=invalid-json showed an error message. | Passed |
| AC-07 | Filtered CSV export | Exported InTransit and checked the header and 2 rows (HUB-1001, HUB-1005). Also checked Exception CSV text with commas and quotes (HUB-1004). | Passed |
| AC-08 | Export failure | Locked locked.csv using PowerShell, then tried to export over it. The app showed Export failed and the file-in-use reason. | Passed |

## CSV sample
sample_export.csv was exported by the app from the local test API (BR-001, All).