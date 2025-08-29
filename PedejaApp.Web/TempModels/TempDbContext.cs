using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PedejaApp.Web.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Addon> Addons { get; set; }

    public virtual DbSet<AddonGroup> AddonGroups { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderItemAddon> OrderItemAddons { get; set; }

    public virtual DbSet<OrderRejection> OrderRejections { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentRefund> PaymentRefunds { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAddonGroup> ProductAddonGroups { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Addon>(entity =>
        {
            entity.HasIndex(e => e.AddonGroupId, "IX_Addons_AddonGroupId");

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(d => d.AddonGroup).WithMany(p => p.Addons).HasForeignKey(d => d.AddonGroupId);
        });

        modelBuilder.Entity<AddonGroup>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_AddonGroups_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "IX_Categories_DisplayOrder");

            entity.HasIndex(e => e.IsActive, "IX_Categories_IsActive");

            entity.HasIndex(e => e.Name, "IX_Categories_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Orders_CreatedAt");

            entity.HasIndex(e => e.CustomerPhone, "IX_Orders_CustomerPhone");

            entity.HasIndex(e => e.Status, "IX_Orders_Status");

            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.DeliveryFee).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.Total).HasPrecision(10, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductId");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItemAddon>(entity =>
        {
            entity.HasIndex(e => e.AddonId, "IX_OrderItemAddons_AddonId");

            entity.HasIndex(e => e.OrderItemId, "IX_OrderItemAddons_OrderItemId");

            entity.Property(e => e.AddonName).HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.Addon).WithMany(p => p.OrderItemAddons)
                .HasForeignKey(d => d.AddonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderItemAddons).HasForeignKey(d => d.OrderItemId);
        });

        modelBuilder.Entity<OrderRejection>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderRejections_OrderId").IsUnique();

            entity.HasIndex(e => e.RejectedAt, "IX_OrderRejections_RejectedAt");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(100);
            entity.Property(e => e.RejectedBy).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithOne(p => p.OrderRejection).HasForeignKey<OrderRejection>(d => d.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistory");

            entity.HasIndex(e => e.OrderId, "IX_OrderStatusHistory_OrderId");

            entity.HasIndex(e => e.Status, "IX_OrderStatusHistory_Status");

            entity.HasIndex(e => e.Timestamp, "IX_OrderStatusHistory_Timestamp");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Payments_CreatedAt");

            entity.HasIndex(e => e.GatewayTransactionId, "IX_Payments_GatewayTransactionId");

            entity.HasIndex(e => e.OrderId, "IX_Payments_OrderId");

            entity.HasIndex(e => e.Status, "IX_Payments_Status");

            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.GatewayResponse).HasMaxLength(1000);
            entity.Property(e => e.GatewayTransactionId).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<PaymentRefund>(entity =>
        {
            entity.HasIndex(e => e.PaymentId, "IX_PaymentRefunds_PaymentId");

            entity.HasIndex(e => e.RefundedAt, "IX_PaymentRefunds_RefundedAt");

            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.GatewayRefundId).HasMaxLength(200);
            entity.Property(e => e.GatewayResponse).HasMaxLength(1000);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RefundedBy).HasMaxLength(100);

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentRefunds).HasForeignKey(d => d.PaymentId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.DisplayOrder, "IX_Products_DisplayOrder");

            entity.HasIndex(e => e.IsActive, "IX_Products_IsActive");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductAddonGroup>(entity =>
        {
            entity.HasIndex(e => e.AddonGroupId, "IX_ProductAddonGroups_AddonGroupId");

            entity.HasIndex(e => new { e.ProductId, e.AddonGroupId }, "IX_ProductAddonGroups_ProductId_AddonGroupId").IsUnique();

            entity.HasOne(d => d.AddonGroup).WithMany(p => p.ProductAddonGroups).HasForeignKey(d => d.AddonGroupId);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAddonGroups).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasIndex(e => e.IsPrimary, "IX_ProductImages_IsPrimary");

            entity.HasIndex(e => e.ProductId, "IX_ProductImages_ProductId");

            entity.HasIndex(e => e.UploadDate, "IX_ProductImages_UploadDate");

            entity.Property(e => e.AspectRatio).HasMaxLength(50);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.LargePath).HasMaxLength(500);
            entity.Property(e => e.MediumPath).HasMaxLength(500);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.OriginalName).HasMaxLength(255);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages).HasForeignKey(d => d.ProductId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
