using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

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
    }
}