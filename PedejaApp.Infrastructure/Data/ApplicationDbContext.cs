using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    // TODO: Add ProductPriceHistory and ProductActivityLog after migration
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
    public DbSet<OrderRejection> OrderRejections { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }
    public DbSet<AddonGroup> AddonGroups { get; set; }
    public DbSet<Addon> Addons { get; set; }
    public DbSet<ProductAddonGroup> ProductAddonGroups { get; set; }
    public DbSet<OrderItemAddon> OrderItemAddons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            entity.Property(e => e.DisplayOrder)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Index for performance (simplified for InMemory)
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Name)
                .IsUnique();

            // Configure relationships
            entity.HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            entity.Property(e => e.Price)
                .IsRequired()
                .HasPrecision(10, 2);
            entity.Property(e => e.DisplayOrder)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired();
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Index for performance (simplified for InMemory)
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CustomerPhone)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(100);
            entity.Property(e => e.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.DeliveryFee)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.Status)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);
            entity.Property(e => e.IsDelivery)
                .IsRequired();

            // Indexes for performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.CustomerPhone);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.Quantity)
                .IsRequired();
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
        });

        // OrderStatusHistory configuration
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId)
                .IsRequired();
            entity.Property(e => e.Status)
                .IsRequired();
            entity.Property(e => e.Timestamp)
                .IsRequired();
            entity.Property(e => e.Notes)
                .HasMaxLength(500);
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(50);

            // Relationships
            entity.HasOne(e => e.Order)
                .WithMany(e => e.StatusHistory)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Timestamp);
        });

        // OrderRejection configuration
        modelBuilder.Entity<OrderRejection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId)
                .IsRequired();
            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Notes)
                .HasMaxLength(500);
            entity.Property(e => e.RejectedAt)
                .IsRequired();
            entity.Property(e => e.RejectedBy)
                .IsRequired()
                .HasMaxLength(50);

            // Relationships
            entity.HasOne(e => e.Order)
                .WithOne(e => e.Rejection)
                .HasForeignKey<OrderRejection>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.RejectedAt);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId)
                .IsRequired();
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            entity.Property(e => e.Method)
                .IsRequired();
            entity.Property(e => e.Status)
                .IsRequired();
            entity.Property(e => e.GatewayTransactionId)
                .HasMaxLength(200);
            entity.Property(e => e.GatewayResponse)
                .HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Refunds)
                .WithOne(e => e.Payment)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.GatewayTransactionId);
        });

        // PaymentRefund configuration
        modelBuilder.Entity<PaymentRefund>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentId)
                .IsRequired();
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.RefundedAt)
                .IsRequired();
            entity.Property(e => e.RefundedBy)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.GatewayRefundId)
                .HasMaxLength(200);
            entity.Property(e => e.GatewayResponse)
                .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Payment)
                .WithMany(e => e.Refunds)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.RefundedAt);
        });

        // AddonGroup configuration
        modelBuilder.Entity<AddonGroup>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Addon configuration
        modelBuilder.Entity<Addon>(entity =>
        {
            entity.HasOne(d => d.AddonGroup)
                  .WithMany(p => p.Addons)
                  .HasForeignKey(d => d.AddonGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductAddonGroup configuration
        modelBuilder.Entity<ProductAddonGroup>(entity =>
        {
            entity.HasOne(d => d.Product)
                  .WithMany(p => p.AddonGroups)
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.AddonGroup)
                  .WithMany(p => p.ProductAddonGroups)
                  .HasForeignKey(d => d.AddonGroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ProductId, e.AddonGroupId }).IsUnique();
        });

        // OrderItemAddon configuration
        modelBuilder.Entity<OrderItemAddon>(entity =>
        {
            entity.HasOne(d => d.OrderItem)
                  .WithMany(p => p.Addons)
                  .HasForeignKey(d => d.OrderItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Addon)
                  .WithMany(p => p.OrderItemAddons)
                  .HasForeignKey(d => d.AddonId)
                  .OnDelete(DeleteBehavior.Restrict); // Não deletar addon se usado em pedido
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed categories
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Pratos Principais",
                Description = "Pratos principais do cardápio",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Category
            {
                Id = 2,
                Name = "Bebidas",
                Description = "Bebidas e refrescos",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Category
            {
                Id = 3,
                Name = "Sobremesas",
                Description = "Doces e sobremesas",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Category
            {
                Id = 4,
                Name = "Entradas",
                Description = "Aperitivos e entradas",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        // Seed products
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Hambúrguer Clássico",
                Description = "Hambúrguer artesanal com carne bovina, queijo, alface, tomate e molho especial",
                Price = 25.90m,
                CategoryId = 1,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Product
            {
                Id = 2,
                Name = "Pizza Margherita",
                Description = "Pizza tradicional com molho de tomate, mussarela e manjericão fresco",
                Price = 32.50m,
                CategoryId = 1,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Product
            {
                Id = 3,
                Name = "Refrigerante Lata",
                Description = "Refrigerante gelado em lata de 350ml - diversos sabores",
                Price = 5.50m,
                CategoryId = 2,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Product
            {
                Id = 4,
                Name = "Suco Natural",
                Description = "Suco natural de frutas frescas - 400ml",
                Price = 8.90m,
                CategoryId = 2,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Product
            {
                Id = 5,
                Name = "Pudim de Leite",
                Description = "Pudim caseiro de leite condensado com calda de açúcar",
                Price = 12.90m,
                CategoryId = 3,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Product
            {
                Id = 6,
                Name = "Porção de Batata Frita",
                Description = "Batatas crocantes cortadas na casa, tempero especial - serve até 3 pessoas",
                Price = 18.50m,
                CategoryId = 4,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        // ProductImage configuration
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.OriginalName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.MimeType)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.FileSize)
                .IsRequired();
            entity.Property(e => e.AspectRatio)
                .HasMaxLength(50);
            entity.Property(e => e.UploadDate)
                .IsRequired();
            entity.Property(e => e.ThumbnailPath)
                .HasMaxLength(500);
            entity.Property(e => e.MediumPath)
                .HasMaxLength(500);
            entity.Property(e => e.LargePath)
                .HasMaxLength(500);

            // Foreign key relationship
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.IsPrimary);
            entity.HasIndex(e => e.UploadDate);
        });

        // TODO: Add ProductPriceHistory and ProductActivityLog configuration after migration
        /*
        // ProductPriceHistory configuration
        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId)
                .IsRequired();
            entity.Property(e => e.PreviousPrice)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.NewPrice)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.ChangedAt)
                .IsRequired();
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(100);
            entity.Property(e => e.Reason)
                .HasMaxLength(200);

            // Relationships
            entity.HasOne(e => e.Product)
                .WithMany(p => p.PriceHistory)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ChangedAt);
        });

        // ProductActivityLog configuration
        modelBuilder.Entity<ProductActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId)
                .IsRequired();
            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.PreviousValue)
                .HasMaxLength(200);
            entity.Property(e => e.NewValue)
                .HasMaxLength(200);
            entity.Property(e => e.Timestamp)
                .IsRequired();
            entity.Property(e => e.UserId)
                .HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ActivityLogs)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.Timestamp);
        });
        */
    }
}
