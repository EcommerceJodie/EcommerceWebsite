using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class ProductImageConfiguration : BaseEntityConfiguration<ProductImage>
    {
        public override void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            base.Configure(builder);

            builder.ToTable("ProductImages");

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(pi => pi.ImageAltText)
                .HasMaxLength(100);

            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 