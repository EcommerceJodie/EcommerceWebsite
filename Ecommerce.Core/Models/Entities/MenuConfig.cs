using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models.Entities
{
    public class MenuConfig : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; }
        public string CustomName { get; set; }
        public string Icon { get; set; }
        public bool IsMainMenu { get; set; } = true;
        

        public Guid? ParentId { get; set; }
        public virtual MenuConfig Parent { get; set; }
        public virtual ICollection<MenuConfig> Children { get; set; } = new List<MenuConfig>();
    }
} 
