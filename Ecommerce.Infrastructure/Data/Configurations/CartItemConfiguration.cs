using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class CartItemConfiguration : BaseEntityConfiguration<CartItem>
    {
        public override void Configure(EntityTypeBuilder<CartItem> builder)
        {
            base.Configure(builder);

            builder.ToTable("CartItems");

            builder.Property(ci => ci.Quantity)
                .IsRequired();

            builder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ci => ci.DateAdded)
                .IsRequired();

            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        }
    }
} 