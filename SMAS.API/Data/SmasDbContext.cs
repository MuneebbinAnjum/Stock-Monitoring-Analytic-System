using Microsoft.EntityFrameworkCore;
using SMAS.API.Models;

namespace SMAS.API.Data
{
    public class SmasDbContext : DbContext
    {
        public SmasDbContext(DbContextOptions<SmasDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintMessage> ComplaintMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRead> NotificationReads { get; set; }
        public DbSet<SaleRecord> SaleRecords { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<ForecastRecord> ForecastRecords { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set;}
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft delete query filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<ProductImage>().HasQueryFilter(pi => !pi.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => !oi.IsDeleted);
            modelBuilder.Entity<Commission>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Discount>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Complaint>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<SaleRecord>().HasQueryFilter(sr => !sr.IsDeleted);
            modelBuilder.Entity<StockAlert>().HasQueryFilter(sa => !sa.IsDeleted);
            modelBuilder.Entity<ForecastRecord>().HasQueryFilter(fr => !fr.IsDeleted);
            modelBuilder.Entity<CartItem>().HasQueryFilter(ci => !ci.IsDeleted);
            modelBuilder.Entity<WishlistItem>().HasQueryFilter(wi => !wi.IsDeleted);
            modelBuilder.Entity<SystemSetting>().HasQueryFilter(ss => !ss.IsDeleted);

            // ── Product relationships ──
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── ProductImage relationships ──
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Category self-referencing (subcategories) ──
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Order relationships ──
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── OrderItem relationships ──
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Commission relationships ──
            modelBuilder.Entity<Commission>()
                .HasOne(c => c.Employee)
                .WithMany()
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Commission>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Discount relationships ──
            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Complaint relationships ──
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Order)
                .WithMany(o => o.Complaints)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Complaints)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Notification relationships ──
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Employee)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany()
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── NotificationRead relationships ──
            modelBuilder.Entity<NotificationRead>()
                .HasOne(nr => nr.Notification)
                .WithMany()
                .HasForeignKey(nr => nr.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── SaleRecord relationships ──
            modelBuilder.Entity<SaleRecord>()
                .HasOne(sr => sr.Product)
                .WithMany(p => p.SaleRecords)
                .HasForeignKey(sr => sr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleRecord>()
                .HasOne(sr => sr.Employee)
                .WithMany(e => e.SaleRecords)
                .HasForeignKey(sr => sr.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── StockAlert relationships ──
            modelBuilder.Entity<StockAlert>()
                .HasOne(sa => sa.Product)
                .WithMany()
                .HasForeignKey(sa => sa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── ForecastRecord relationships ──
            modelBuilder.Entity<ForecastRecord>()
                .HasOne(fr => fr.Product)
                .WithMany()
                .HasForeignKey(fr => fr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── CartItem relationships ──
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Customer)
                .WithMany()
                .HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── WishlistItem relationships ──
            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Customer)
                .WithMany()
                .HasForeignKey(wi => wi.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── InventoryTransaction relationships ──
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(it => it.Product)
                .WithMany()
                .HasForeignKey(it => it.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Indexes ──
            modelBuilder.Entity<Product>().HasIndex(p => p.CategoryId);
            modelBuilder.Entity<Product>().HasIndex(p => p.SupplierId);
            modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Status);
            modelBuilder.Entity<Product>().HasIndex(p => p.IsFeatured);

            modelBuilder.Entity<Order>().HasIndex(o => o.CustomerId);
            modelBuilder.Entity<Order>().HasIndex(o => o.EmployeeId);
            modelBuilder.Entity<Order>().HasIndex(o => o.Status);
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

            modelBuilder.Entity<OrderItem>().HasIndex(oi => oi.OrderId);
            modelBuilder.Entity<OrderItem>().HasIndex(oi => oi.ProductId);

            modelBuilder.Entity<Commission>().HasIndex(c => c.EmployeeId);
            modelBuilder.Entity<Commission>().HasIndex(c => c.ProductId);
            modelBuilder.Entity<Commission>().HasIndex(c => new { c.EmployeeId, c.ProductId }).IsUnique();

            modelBuilder.Entity<Discount>().HasIndex(d => d.ProductId);
            modelBuilder.Entity<Discount>().HasIndex(d => d.StartDate);
            modelBuilder.Entity<Discount>().HasIndex(d => d.EndDate);

            modelBuilder.Entity<Complaint>().HasIndex(c => c.OrderId);
            modelBuilder.Entity<Complaint>().HasIndex(c => c.CustomerId);
            modelBuilder.Entity<Complaint>().HasIndex(c => c.Status);

            // ComplaintMessage relationships
            modelBuilder.Entity<ComplaintMessage>()
                .HasOne(cm => cm.Complaint)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComplaintMessage>().HasIndex(cm => cm.ComplaintId);

            modelBuilder.Entity<Notification>().HasIndex(n => n.EmployeeId);
            modelBuilder.Entity<Notification>().HasIndex(n => n.CustomerId);
            modelBuilder.Entity<Notification>().HasIndex(n => n.IsRead);
            modelBuilder.Entity<NotificationRead>().HasIndex(nr => new { nr.NotificationId, nr.UserId, nr.UserType }).IsUnique();

            modelBuilder.Entity<SaleRecord>().HasIndex(sr => sr.ProductId);
            modelBuilder.Entity<SaleRecord>().HasIndex(sr => sr.EmployeeId);
            modelBuilder.Entity<SaleRecord>().HasIndex(sr => sr.SaleDate);

            modelBuilder.Entity<StockAlert>().HasIndex(sa => sa.ProductId);
            modelBuilder.Entity<StockAlert>().HasIndex(sa => sa.IsResolved);
            // Prevent multiple unresolved alerts per product
            modelBuilder.Entity<StockAlert>().HasIndex(sa => new { sa.ProductId, sa.IsResolved }).IsUnique();

            modelBuilder.Entity<ForecastRecord>().HasIndex(fr => fr.ForecastDate);

            modelBuilder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();

            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.Token).IsUnique();

            modelBuilder.Entity<CartItem>().HasIndex(ci => new { ci.CustomerId, ci.ProductId }).IsUnique();
            modelBuilder.Entity<WishlistItem>().HasIndex(wi => new { wi.CustomerId, wi.ProductId }).IsUnique();

            modelBuilder.Entity<SystemSetting>().HasIndex(ss => ss.Key).IsUnique();

            modelBuilder.Entity<AuditLog>().HasIndex(al => al.EntityName);
            modelBuilder.Entity<AuditLog>().HasIndex(al => al.PerformedAt);

            // Automatic timestamps
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("UpdatedAt")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnUpdate();
                }
            }
        }
    }
}