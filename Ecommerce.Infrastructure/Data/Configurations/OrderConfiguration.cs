using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : BaseEntityConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);

            builder.ToTable("Orders");

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.ShippingAddress)
                .HasMaxLength(255);

            builder.Property(o => o.ShippingCity)
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPostalCode)
                .HasMaxLength(20);

            builder.Property(o => o.ShippingCountry)
                .HasMaxLength(50);

            builder.Property(o => o.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(o => o.PaymentTransactionId)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(o => o.Notes)
                .HasMaxLength(500);

            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();
        }
    }
} 
