using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
} 