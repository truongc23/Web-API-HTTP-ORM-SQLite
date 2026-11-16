using CampusEquipmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEquipmentApi.Data;

public sealed class EquipmentDbContext(
    DbContextOptions<EquipmentDbContext> options) : DbContext(options)
{
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentItem>(entity =>
        {
            entity.ToTable("EquipmentItems");
            entity.HasKey(item => item.EquipmentItemId);
            entity.Property(item => item.Name).IsRequired().HasMaxLength(100);
            entity.Property(item => item.Category).IsRequired().HasMaxLength(50);
        });
    }
}
