# Campus Equipment API

A .NET 10 Minimal API for listing, creating, updating, and deleting campus equipment. EF Core persists the records in SQLite; the initial migration and repeat-safe seed callback prepare the local database.

## Prerequisites and tested environment

- Stable .NET SDK 10.0.404 (global.json requests 10.0.403 and permits the latest .NET 10 feature/patch roll-forward)
- VS Code with C# Dev Kit, REST Client, and SQLite extensions
- Windows (the project was built and exercised in this workspace)

The project pins EF Core and `dotnet-ef` to 10.0.14. SQLite is an embedded file database; no server installation is needed.

## Run from a fresh clone

Run these commands from the project root:

```powershell
dotnet --version
dotnet tool restore
dotnet restore
dotnet build
dotnet ef database update
dotnet run --no-launch-profile --urls http://localhost:5050
```

The database file is `campus-equipment.db` in the project root and is ignored by Git. Run HTTP checks in VS Code with REST Client using [CampusEquipmentApi.http](./CampusEquipmentApi.http). Read the first collection response and set `@seedId` to one of its actual IDs before request 02. Send request 03 before using the `@createdId` response variable. Pause after request 14 to inspect SQLite and restart the API before request 15.

## Request results

The expected statuses below were verified against the running API using HTTP requests, including a stop/restart between requests 14 and 15. Replay the requests in REST Client and add your screenshots before submitting; automated checks are not a substitute for the required course evidence.

| Request | Expected | Actual | Observation | Evidence |
|---:|---:|---|---|---|
| 01 | 200 | 200 | Three seeded items returned with IDs 1, 2, and 3. | Capture required |
| 02 | 200 | 200 | Seed item 1 returned. | Capture required |
| 03 | 201 | 201 | Item 4 created; response included `Location` and `id`. | Capture required |
| 04 | 200 | 200 | Created item returned with original values. | Capture required |
| 05 | 204 | 204 | Empty response body. | Capture required |
| 06 | 200 | 200 | Updated name, category, and availability returned. | Capture required |
| 07 | 400 | 400 | `errors.name` rejected a whitespace-only name. | Capture required |
| 08 | 400 | 400 | `errors.category` rejected an omitted category. | Capture required |
| 09 | 400 | 400 | `errors.name` rejected a 101-character name. | Capture required |
| 10 | 400 | 400 | `errors.category` rejected a whitespace-only category. | Capture required |
| 11 | 400 | 400 | `errors.category` rejected a 51-character category. | Capture required |
| 12 | 400 | 400 | `errors.name` rejected a null name. | Capture required |
| 13 | 200 | 200 | Invalid PUT requests did not change the item. | Capture required |
| 14 | 200 | 200 | Four items before restart: three seeds plus the test item. | Capture required |
| 15 | 200 | 200 | Item 4 retained the updated values after server restart. | Capture required |
| 16 | 204 | 204 | Empty response body; item 4 deleted. | Capture required |
| 17 | 404 | 404 | Deleted item 4 was not found. | Capture required |
| 18 | 404 | 404 | Valid update of deleted item 4 was not found. | Capture required |
| 19 | 404 | 404 | Repeated deletion of item 4 was not found. | Capture required |
| 20 | 200 | 200 | Only the three seed items remained. | Capture required |

## Evidence index

Save screenshots in `evidence/` and link them in the results table above. Capture your own execution output; screenshots are not included until you run the checks.

| ID | Evidence to capture |
|---|---|
| E1 | SDK version, EF tool/package versions, and successful build |
| E2 | Generated migration files, successful database update, and applied migration listing |
| E3 | Seed rows/count before and after a second database update, including update output |
| E4 | Collection/single GET, POST with `Location`, PUT, and DELETE responses; show empty bodies for 204 |
| E5 | GET after update, after restart (same item ID), after delete, and final collection |
| E6 | Responses for requests 07–12, 17–19, and unchanged data from requests 13–14 |
| E7 | SQLite schema, migration history, updated row at the pause, and final deletion/count |
| E8 | Repository URL and tracked files, README explanations, results table, and evidence index |

## Concepts

- **DTO vs. entity:** `EquipmentItemRequest` contains only editable request fields, while `EquipmentItem` represents a persisted row. The database generates `EquipmentItemId`, so clients do not supply it in the DTO.
- **PUT, DI, and persistence:** ASP.NET Core injects a scoped `EquipmentDbContext` into the handler. EF Core tracks the found entity; the handler updates its fields and `SaveChangesAsync` persists those tracked changes.
- **Migration generation vs. application:** `dotnet ef migrations add` creates versioned schema-change code from the model. `dotnet ef database update` applies pending migrations to the SQLite file and records them in `__EFMigrationsHistory`.
- **Repeat-safe seeding:** The seed callback inserts the sample rows only when the equipment table is empty, so applying migrations again does not duplicate rows.
- **Restart check:** Reading the same changed item after stopping and restarting the server demonstrates that the data was persisted to the SQLite file, rather than held only in server memory.
- **String length validation:** The model's maximum lengths describe the schema to EF Core, but SQLite does not enforce those limits for these columns. The API validates lengths before saving to guarantee the request contract.
