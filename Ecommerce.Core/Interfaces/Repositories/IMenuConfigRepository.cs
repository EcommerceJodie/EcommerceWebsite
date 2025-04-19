using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface IMenuConfigRepository : IRepository<MenuConfig>
    {
        Task<IEnumerable<MenuConfig>> GetVisibleMenuConfigsAsync(bool isMainMenu = true);
        Task<IEnumerable<MenuConfig>> GetRootMenuConfigsAsync(bool isMainMenu = true);
        Task<IEnumerable<MenuConfig>> GetByParentIdAsync(Guid? parentId);
        Task<MenuConfig> GetByCategoryIdAsync(Guid categoryId, bool isMainMenu = true);
    }
} 
