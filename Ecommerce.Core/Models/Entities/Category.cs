using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models.Entities
{
    public class Category : BaseEntity
    {
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string CategorySlug { get; set; }
        public string CategoryImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 