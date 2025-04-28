using System;
using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{
    public class DeleteMultipleProductsDto
    {
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
    }
} 