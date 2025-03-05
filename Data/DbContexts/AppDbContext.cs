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
        modelBuilder.Entity<Component>().ToTable("Components", "fam");
        modelBuilder.Entity<ComponentItem>().ToTable("ComponentItems", "fam");
        modelBuilder.Entity<ComponentType>().ToTable("ComponentTypes", "fam");
        modelBuilder.Entity<ComponentTypeAttribute>().ToTable("ComponentTypeAttributes", "fam");
        modelBuilder.Entity<ComponentTypeAttributeAssoc>().ToTable("ComponentTypeAttributeAssocs", "fam");
        modelBuilder.Entity<Content>().ToTable("Content", "fam");
        modelBuilder.Entity<ContentCategory>().ToTable("ContentCategories", "fam");
        modelBuilder.Entity<ContentCategoryEmployeeAssoc>().ToTable("ContentCategoryEmployeeAssocs", "fam");
        modelBuilder.Entity<ContentCategoryEmployeeRecord>().ToTable("ContentCategoryEmployeeRecords", "fam");
        modelBuilder.Entity<ContentComponentAssoc>().ToTable("ContentComponentAssocs", "fam");

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