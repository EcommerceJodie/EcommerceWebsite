using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Ecommerce.Infrastructure.Data.Configurations
{
    public class MenuConfigConfiguration : IEntityTypeConfiguration<MenuConfig>
    {
        public void Configure(EntityTypeBuilder<MenuConfig> builder)
        {
            builder.ToTable("MenuConfigs");
            
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.CustomName)
                .HasMaxLength(100);
            
            builder.Property(m => m.Icon)
                .HasMaxLength(50);
            
            builder.HasOne(m => m.Category)
                .WithMany()
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasIndex(m => new { m.CategoryId, m.IsMainMenu, m.ParentId });
        }
    }
} 
