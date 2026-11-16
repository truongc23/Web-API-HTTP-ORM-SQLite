using CampusEquipmentApi.Contracts;
using CampusEquipmentApi.Data;
using CampusEquipmentApi.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("EquipmentDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'EquipmentDatabase' is missing.");

var sqliteConnection = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(sqliteConnection.DataSource))
{
    sqliteConnection.DataSource = Path.Combine(
        builder.Environment.ContentRootPath, sqliteConnection.DataSource);
}

builder.Services.AddDbContext<EquipmentDbContext>(options =>
    options.UseSqlite(sqliteConnection.ToString())
        .UseSeeding((context, _) =>
        {
            if (!context.Set<EquipmentItem>().Any())
            {
                context.Set<EquipmentItem>().AddRange(
                    new EquipmentItem
                    {
                        Name = "Dell Latitude Laptop",
                        Category = "Computers",
                        IsAvailable = true
                    },
                    new EquipmentItem
                    {
                        Name = "Epson Projector",
                        Category = "Presentation",
                        IsAvailable = true
                    },
                    new EquipmentItem
                    {
                        Name = "Canon Camera",
                        Category = "Photography",
                        IsAvailable = false
                    });
                context.SaveChanges();
            }
        }));

var app = builder.Build();

app.MapGet("/equipment", async (EquipmentDbContext db) =>
    Results.Ok(await db.EquipmentItems
        .AsNoTracking()
        .OrderBy(item => item.EquipmentItemId)
        .ToListAsync()));

app.MapGet("/equipment/{id:int}", async Task<IResult> (
    int id, EquipmentDbContext db) =>
{
    var item = await db.EquipmentItems.AsNoTracking()
        .SingleOrDefaultAsync(item => item.EquipmentItemId == id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/equipment", async Task<IResult> (
    EquipmentItemRequest request, EquipmentDbContext db) =>
{
    var errors = ValidateItem(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var item = new EquipmentItem
    {
        Name = request.Name!,
        Category = request.Category!,
        IsAvailable = request.IsAvailable
    };

    db.EquipmentItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/equipment/{item.EquipmentItemId}", item);
});

app.MapPut("/equipment/{id:int}", async Task<IResult> (
    int id, EquipmentItemRequest request, EquipmentDbContext db) =>
{
    var errors = ValidateItem(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var item = await db.EquipmentItems.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    item.Name = request.Name!;
    item.Category = request.Category!;
    item.IsAvailable = request.IsAvailable;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/equipment/{id:int}", async Task<IResult> (
    int id, EquipmentDbContext db) =>
{
    var item = await db.EquipmentItems.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    db.EquipmentItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

static Dictionary<string, string[]> ValidateItem(EquipmentItemRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        errors["name"] = ["Name is required."];
    }
    else if (request.Name.Length > 100)
    {
        errors["name"] = ["Name must be 100 characters or fewer."];
    }

    if (string.IsNullOrWhiteSpace(request.Category))
    {
        errors["category"] = ["Category is required."];
    }
    else if (request.Category.Length > 50)
    {
        errors["category"] = ["Category must be 50 characters or fewer."];
    }

    return errors;
}
