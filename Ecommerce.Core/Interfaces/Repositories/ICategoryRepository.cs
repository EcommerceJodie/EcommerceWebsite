using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category> GetBySlugAsync(string slug);
        Task<bool> IsCategorySlugUniqueAsync(string slug, Guid? excludeId = null);
    }
} 