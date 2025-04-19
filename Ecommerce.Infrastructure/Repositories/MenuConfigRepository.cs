using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class MenuConfigRepository : Repository<MenuConfig>, IMenuConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public MenuConfigRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuConfig>> GetVisibleMenuConfigsAsync(bool isMainMenu = true)
        {
            return await _context.MenuConfigs
                .Include(m => m.Category)
                .Include(m => m.Children.Where(c => !c.IsDeleted && c.IsVisible))
                    .ThenInclude(c => c.Category)
                .Where(m => !m.IsDeleted && m.IsVisible && m.IsMainMenu == isMainMenu 
                       && !m.Category.IsDeleted && m.Category.IsActive
                       && m.ParentId == null) 
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuConfig>> GetRootMenuConfigsAsync(bool isMainMenu = true)
        {
            return await _context.MenuConfigs
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted && m.IsMainMenu == isMainMenu && m.ParentId == null)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuConfig>> GetByParentIdAsync(Guid? parentId)
        {
            return await _context.MenuConfigs
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted && m.ParentId == parentId)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MenuConfig> GetByCategoryIdAsync(Guid categoryId, bool isMainMenu = true)
        {
            return await _context.MenuConfigs
                .FirstOrDefaultAsync(m => m.CategoryId == categoryId && m.IsMainMenu == isMainMenu && !m.IsDeleted);
        }
    }
} 
