# Verification Checklist – HubLink (PT5)

Fill in the **Result** column while testing the running app. Mark ✔ Passed / ✘ Failed / ☐ Not done.

| ID | Scenario | How we checked it | Result |
|---|---|---|---|
| AC-01 | Retrieve shipments for configured branch | Started the local service, ran the app with BR-001. Compared the table with the browser result at `http://127.0.0.1:5080/api/shipments?branchCode=BR-001` (5 shipments). | ☐ |
| AC-02 | Status filters | Selected each status, then All. Expected BR-001 rows: Pending 1, InTransit 2, Delivered 1, Exception 1, All 5. | ☐ |
| AC-03 | Summary | Summary showed Total 5, Pending 1, InTransit 2, Delivered 1, Exception 1 and did not change when the filter changed. | ☐ |
| AC-04 | Refresh / loading | Clicked Refresh (used `mode=delay` with a longer timeout to see it). Progress bar shown, window still movable, buttons disabled while loading. | ☐ |
| AC-05 | Unavailable / timeout / HTTP error | Tested: service stopped, `mode=delay`, `mode=bad-request` (400), `mode=server-error` (500). Red error message shown, Retry button appeared, no "success" message. Old data (if any) marked as "loaded earlier". | ☐ |
| AC-06 | Empty / invalid response | `mode=empty` → "no shipments" message (not an error). `mode=invalid-json` → error message. Also checked BR-999 → empty. | ☐ |
| AC-07 | CSV export with filter | Filtered to InTransit, exported, opened the CSV in Notepad: correct header and 2 rows, values with commas/quotes properly escaped. | ☐ |
| AC-08 | Export failure | Tried saving to a location that is not writable / file open in another program. Red "Export failed" message shown, no success message. | ☐ |

## Notes / Incomplete items
- _(Write any scenario that failed or was not tested here.)_

## CSV sample
`sample_export.csv` – produced by the app (BR-001, status All) from the local test API.
