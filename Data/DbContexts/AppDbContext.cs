using Data.Entities;
using Data.Entities.dbo;
using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    #region dbo

    public DbSet<Employee> Employees { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // [fam]
        modelBuilder.Entity<Component>().ToTable("Component", "fam");
        modelBuilder.Entity<ComponentAttributeAssoc>().ToTable("ComponentAttributeAssoc", "fam");
        modelBuilder.Entity<ComponentItem>().ToTable("ComponentItem", "fam");
        modelBuilder.Entity<ComponentType>().ToTable("ComponentType", "fam");
        modelBuilder.Entity<ComponentTypeAttribute>().ToTable("ComponentTypeAttribute", "fam");
        modelBuilder.Entity<ComponentTypeAttributeAssoc>().ToTable("ComponentTypeAttributeAssoc", "fam");
        modelBuilder.Entity<Content>().ToTable("Content", "fam");
        modelBuilder.Entity<ContentCategory>().ToTable("ContentCategory", "fam");
        modelBuilder.Entity<ContentCategoryEmployeeAssoc>().ToTable("ContentCategoryEmployeeAssoc", "fam");
        modelBuilder.Entity<ContentCategoryEmployeeRecord>().ToTable("ContentCategoryEmployeeRecord", "fam");
        modelBuilder.Entity<ContentComponentAssoc>().ToTable("ContentComponentAssoc", "fam");

        // [dbo]
        modelBuilder.Entity<Employee>().ToTable("Employee", "dbo");

        base.OnModelCreating(modelBuilder);
    }

    #region fam

    public DbSet<Component> Components { get; set; }
    public DbSet<ComponentItem> ComponentItems { get; set; }
    public DbSet<ComponentType> ComponentTypes { get; set; }
    public DbSet<ComponentTypeAttribute> ComponentTypeAttributes { get; set; }
    public DbSet<ComponentTypeAttributeAssoc> ComponentTypeAttributeAssocs { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<ContentCategory> ContentCategories { get; set; }
    public DbSet<ContentCategoryEmployeeAssoc> ContentCategoryEmployeeAssocs { get; set; }
    public DbSet<ContentCategoryEmployeeRecord> ContentCategoryEmployeeRecords { get; set; }
    public DbSet<ContentComponentAssoc> ContentComponentAssocs { get; set; }

    #endregion
}