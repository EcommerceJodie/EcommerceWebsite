using System;
using System.Collections.Generic;

namespace Ecommerce.Core.DTOs.Identity
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 