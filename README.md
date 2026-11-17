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

The database file is `campus-equipment.db` in the project root and is ignored by Git. Run HTTP checks in VS Code with REST Client using [CampusEquipmentApi.http](./CampusEquipmentApi.http). Read the first collection response and set `@seedId` to one of its actual IDs before request 02. Send request 03 before using `{{createItem.response.body.equipmentItemId}}` in subsequent requests. Pause after request 14 to inspect SQLite and restart the API before request 15.

## Request results

The screenshots below record REST Client responses. The same API process was running during requests 14 and 15. A separate restart check used item 8, shown [before](./evidence/persistence-before.png) and [after](./evidence/persistence-after.png) restarting the API.

| Request | Expected | Actual | Observation | Evidence |
|---:|---:|---|---|---|
| 01 | 200 | 200 | Three seed items returned. | [Screenshot](./evidence/req-01.png) |
| 02 | 200 | 200 | Seed item 1 returned. | [Screenshot](./evidence/req-02.png) |
| 03 | 201 | 201 | Item 7 created with `Location` and `equipmentItemId`. | [Screenshot](./evidence/req-03.png) |
| 04 | 200 | 200 | Item 7 returned with the original values. | [Screenshot](./evidence/req-04.png) |
| 05 | 204 | 204 | Empty response body. | [Screenshot](./evidence/req-05.png) |
| 06 | 200 | 200 | Updated item 7 returned. | [Screenshot](./evidence/req-06.png) |
| 07 | 400 | 400 | `errors.name` rejected a whitespace-only name. | [Screenshot](./evidence/req-07.png) |
| 08 | 400 | 400 | `errors.category` rejected an omitted category. | [Screenshot](./evidence/req-08.png) |
| 09 | 400 | 400 | `errors.name` rejected a 101-character name. | [Screenshot](./evidence/req-09.png) |
| 10 | 400 | 400 | `errors.category` rejected a whitespace-only category. | [Screenshot](./evidence/req-10.png) |
| 11 | 400 | 400 | `errors.category` rejected a 51-character category. | [Screenshot](./evidence/req-11.png) |
| 12 | 400 | 400 | `errors.name` rejected a null name. | [Screenshot](./evidence/req-12.png) |
| 13 | 200 | 200 | Item 7 still has the updated values. | [Screenshot](./evidence/req-13.png) |
| 14 | 200 | 200 | Four items: three seeds and item 7. | [Screenshot](./evidence/req-14.png) |
| 15 | 200 | 200 | Item 7 has the updated values; restart not performed between 14 and 15. | [Screenshot](./evidence/req-15.png) |
| 16 | 204 | 204 | Empty response body. | [Screenshot](./evidence/req-16.png) |
| 17 | 404 | 404 | Item not found. | [Screenshot](./evidence/req-17.png) |
| 18 | 404 | 404 | Valid update of a missing item was not found. | [Screenshot](./evidence/req-18.png) |
| 19 | 404 | 404 | Repeated deletion was not found. | [Screenshot](./evidence/req-19.png) |
| 20 | 200 | 200 | Only the three seed items remain. | [Screenshot](./evidence/req-20.png) |

## Evidence index

Save screenshots in `evidence/` and link them in the results table above.

| ID | Evidence to capture |
|---|---|
| E1 | [SDK and EF versions, package list, and build output](<./evidence/Screenshot 2026-11-16 134245.png>); [successful build summary](./evidence/e1-build.png) |
| E2 | [Migration files, database update, and applied migration listing](./evidence/e2-migrations.png) |
| E3 | Seed rows [before](./evidence/sqlite-after-delete.png) and [after](./evidence/seed-rows-after.png); count [before](./evidence/seed-count-before.png) and [after](./evidence/seed-count-after.png); [second update output](./evidence/seed-update.png) |
| E4 | Collection/single GET, POST with `Location`, PUT, and DELETE responses: see request screenshots above |
| E5 | GET after update and restart for item 8: [before](./evidence/persistence-before.png), [after](./evidence/persistence-after.png); GET after delete: [17](./evidence/req-17.png); final collection: [20](./evidence/req-20.png) |
| E6 | Responses for requests 07–12, 17–19, and unchanged data from requests 13–14: see request screenshots above |
| E7 | SQLite [schema](./evidence/sqlite-schema.png), [migration history](./evidence/sqlite-migration-history.png), [updated row before restart](./evidence/sqlite-before-restart.png), and [rows after deletion](./evidence/sqlite-after-delete.png) |
| E8 | Repository URL and tracked files, README explanations, results table, and evidence index |

## Concepts

- **DTO vs. entity:** `EquipmentItemRequest` contains only editable request fields, while `EquipmentItem` represents a persisted row. The database generates `EquipmentItemId`, so clients do not supply it in the DTO.
- **PUT, DI, and persistence:** ASP.NET Core injects a scoped `EquipmentDbContext` into the handler. EF Core tracks the found entity; the handler updates its fields and `SaveChangesAsync` persists those tracked changes.
- **Migration generation vs. application:** `dotnet ef migrations add` creates versioned schema-change code from the model. `dotnet ef database update` applies pending migrations to the SQLite file and records them in `__EFMigrationsHistory`.
- **Repeat-safe seeding:** The seed callback inserts the sample rows only when the equipment table is empty, so applying migrations again does not duplicate rows.
- **Restart check:** Reading the same changed item after stopping and restarting the server demonstrates that the data was persisted to the SQLite file, rather than held only in server memory.
- **String length validation:** The model's maximum lengths describe the schema to EF Core, but SQLite does not enforce those limits for these columns. The API validates lengths before saving to guarantee the request contract.
