using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IMenuConfigService
    {
        Task<List<MenuConfigDto>> GetAllMenuConfigsAsync();
        Task<List<MenuConfigDto>> GetVisibleMenuConfigsAsync(bool isMainMenu = true);
        Task<List<MenuConfigDto>> GetRootMenuConfigsAsync(bool isMainMenu = true);
        Task<List<MenuConfigDto>> GetMenuConfigsByParentIdAsync(Guid? parentId);
        Task<MenuConfigDto> GetMenuConfigByIdAsync(Guid id);
        Task<MenuConfigDto> GetByCategoryIdAsync(Guid categoryId, bool isMainMenu = true);
        Task<MenuConfigDto> CreateMenuConfigAsync(CreateMenuConfigDto menuConfigDto);
        Task<MenuConfigDto> UpdateMenuConfigAsync(UpdateMenuConfigDto menuConfigDto);
        Task<bool> DeleteMenuConfigAsync(Guid id);
        Task<List<MenuConfigDto>> GetMenuTreeAsync();
    }
} 
