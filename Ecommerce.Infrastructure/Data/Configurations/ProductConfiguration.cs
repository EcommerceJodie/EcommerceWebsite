using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            builder.ToTable("Products");

            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ProductDescription)
                .HasMaxLength(2000);

            builder.Property(p => p.ProductSlug)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ProductPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.ProductDiscountPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.ProductSku)
                .HasMaxLength(50);

            builder.Property(p => p.MetaTitle)
                .HasMaxLength(100);

            builder.Property(p => p.MetaDescription)
                .HasMaxLength(250);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.ProductSlug)
                .IsUnique();
            
            builder.HasIndex(p => p.ProductSku)
                .IsUnique();
        }
    }
} 