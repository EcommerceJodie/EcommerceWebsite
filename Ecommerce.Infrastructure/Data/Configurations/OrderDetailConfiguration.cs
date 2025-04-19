using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class OrderDetailConfiguration : BaseEntityConfiguration<OrderDetail>
    {
        public override void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            base.Configure(builder);

            builder.ToTable("OrderDetails");

            builder.Property(od => od.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(od => od.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(od => od.Discount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(od => od.Quantity)
                .IsRequired();

            builder.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 
