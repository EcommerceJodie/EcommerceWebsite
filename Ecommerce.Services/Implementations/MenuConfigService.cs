using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class MenuConfigService : IMenuConfigService
    {
        private readonly IMenuConfigRepository _menuConfigRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MenuConfigService(
            IMenuConfigRepository menuConfigRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _menuConfigRepository = menuConfigRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<MenuConfigDto>> GetAllMenuConfigsAsync()
        {
            var menuConfigs = await _menuConfigRepository.GetAllAsync();
            return _mapper.Map<List<MenuConfigDto>>(menuConfigs);
        }

        public async Task<List<MenuConfigDto>> GetVisibleMenuConfigsAsync(bool isMainMenu = true)
        {
            var menuConfigs = await _menuConfigRepository.GetVisibleMenuConfigsAsync(isMainMenu);
            return _mapper.Map<List<MenuConfigDto>>(menuConfigs);
        }

        public async Task<List<MenuConfigDto>> GetRootMenuConfigsAsync(bool isMainMenu = true)
        {
            var menuConfigs = await _menuConfigRepository.GetRootMenuConfigsAsync(isMainMenu);
            return _mapper.Map<List<MenuConfigDto>>(menuConfigs);
        }

        public async Task<List<MenuConfigDto>> GetMenuConfigsByParentIdAsync(Guid? parentId)
        {
            var menuConfigs = await _menuConfigRepository.GetByParentIdAsync(parentId);
            return _mapper.Map<List<MenuConfigDto>>(menuConfigs);
        }

        public async Task<MenuConfigDto> GetMenuConfigByIdAsync(Guid id)
        {
            var menuConfig = await _menuConfigRepository.GetByIdAsync(id);
            if (menuConfig == null || menuConfig.IsDeleted)
            {
                throw new EntityNotFoundException("Cấu hình menu", id);
            }
            return _mapper.Map<MenuConfigDto>(menuConfig);
        }

        public async Task<MenuConfigDto> GetByCategoryIdAsync(Guid categoryId, bool isMainMenu = true)
        {
            var menuConfig = await _menuConfigRepository.GetByCategoryIdAsync(categoryId, isMainMenu);
            if (menuConfig == null || menuConfig.IsDeleted)
            {
                return null;
            }
            return _mapper.Map<MenuConfigDto>(menuConfig);
        }

        public async Task<MenuConfigDto> CreateMenuConfigAsync(CreateMenuConfigDto menuConfigDto)
        {
            var category = await _categoryRepository.GetByIdAsync(menuConfigDto.CategoryId);
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", menuConfigDto.CategoryId);
            }


            if (menuConfigDto.ParentId == null)
            {
                var existingConfig = await _menuConfigRepository.GetByCategoryIdAsync(menuConfigDto.CategoryId, menuConfigDto.IsMainMenu);
                if (existingConfig != null)
                {
                    throw new Exception($"Đã tồn tại cấu hình menu gốc cho danh mục này với loại menu {(menuConfigDto.IsMainMenu ? "chính" : "phụ")}");
                }
            }


            if (menuConfigDto.ParentId.HasValue)
            {
                var parentMenu = await _menuConfigRepository.GetByIdAsync(menuConfigDto.ParentId.Value);
                if (parentMenu == null || parentMenu.IsDeleted)
                {
                    throw new EntityNotFoundException("Menu cha", menuConfigDto.ParentId.Value);
                }
            }

            var menuConfig = _mapper.Map<MenuConfig>(menuConfigDto);
            
            menuConfig.Id = Guid.NewGuid();
            menuConfig.CreatedAt = DateTime.UtcNow;

            bool startedTransaction = false;
            try
            {
                if (!_unitOfWork.HasActiveTransaction())
                {
                    await _unitOfWork.BeginTransactionAsync();
                    startedTransaction = true;
                }

                _menuConfigRepository.Add(menuConfig);
                await _unitOfWork.CompleteAsync();
                
                if (startedTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
                

                var resultDto = new MenuConfigDto
                {
                    Id = menuConfig.Id,
                    CategoryId = menuConfig.CategoryId,
                    CategoryName = category.CategoryName,
                    CategorySlug = category.CategorySlug,
                    CategoryImageUrl = category.CategoryImageUrl,
                    IsVisible = menuConfig.IsVisible,
                    DisplayOrder = menuConfig.DisplayOrder,
                    CustomName = menuConfig.CustomName,
                    Icon = menuConfig.Icon,
                    IsMainMenu = menuConfig.IsMainMenu,
                    ParentId = menuConfig.ParentId,
                    CreatedAt = menuConfig.CreatedAt,
                    UpdatedAt = menuConfig.UpdatedAt,
                    Children = new List<MenuConfigDto>() 
                };
                
                return resultDto;
            }
            catch (Exception)
            {
                if (startedTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<MenuConfigDto> UpdateMenuConfigAsync(UpdateMenuConfigDto menuConfigDto)
        {
            var category = await _categoryRepository.GetByIdAsync(menuConfigDto.CategoryId);
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", menuConfigDto.CategoryId);
            }

            var menuConfig = await _menuConfigRepository.GetByIdAsync(menuConfigDto.Id);
            if (menuConfig == null || menuConfig.IsDeleted)
            {
                throw new EntityNotFoundException("Cấu hình menu", menuConfigDto.Id);
            }


            if (menuConfigDto.ParentId == null && (menuConfig.CategoryId != menuConfigDto.CategoryId || menuConfig.IsMainMenu != menuConfigDto.IsMainMenu))
            {
                var existingConfig = await _menuConfigRepository.GetByCategoryIdAsync(menuConfigDto.CategoryId, menuConfigDto.IsMainMenu);
                if (existingConfig != null && existingConfig.Id != menuConfigDto.Id)
                {
                    throw new Exception($"Đã tồn tại cấu hình menu gốc cho danh mục này với loại menu {(menuConfigDto.IsMainMenu ? "chính" : "phụ")}");
                }
            }


            if (menuConfigDto.ParentId.HasValue)
            {
                var parentMenu = await _menuConfigRepository.GetByIdAsync(menuConfigDto.ParentId.Value);
                if (parentMenu == null || parentMenu.IsDeleted)
                {
                    throw new EntityNotFoundException("Menu cha", menuConfigDto.ParentId.Value);
                }
                

                if (menuConfigDto.ParentId.Value == menuConfigDto.Id || await IsChildOf(menuConfigDto.ParentId.Value, menuConfigDto.Id))
                {
                    throw new Exception("Không thể đặt menu này làm con của chính nó hoặc con của nó");
                }
            }

            _mapper.Map(menuConfigDto, menuConfig);
            
            menuConfig.UpdatedAt = DateTime.UtcNow;

            bool startedTransaction = false;
            try
            {
                if (!_unitOfWork.HasActiveTransaction())
                {
                    await _unitOfWork.BeginTransactionAsync();
                    startedTransaction = true;
                }

                _menuConfigRepository.Update(menuConfig);
                await _unitOfWork.CompleteAsync();
                
                if (startedTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                if (startedTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }

            return _mapper.Map<MenuConfigDto>(menuConfig);
        }

        public async Task<bool> DeleteMenuConfigAsync(Guid id)
        {
            var menuConfig = await _menuConfigRepository.GetByIdAsync(id);
            
            if (menuConfig == null || menuConfig.IsDeleted)
            {
                throw new EntityNotFoundException("Cấu hình menu", id);
            }


            var childMenus = await _menuConfigRepository.GetByParentIdAsync(id);
            if (childMenus.Any())
            {
                throw new Exception("Không thể xóa menu này vì có menu con phụ thuộc. Vui lòng xóa các menu con trước.");
            }

            menuConfig.IsDeleted = true;
            menuConfig.UpdatedAt = DateTime.UtcNow;

            bool startedTransaction = false;
            try
            {
                if (!_unitOfWork.HasActiveTransaction())
                {
                    await _unitOfWork.BeginTransactionAsync();
                    startedTransaction = true;
                }

                _menuConfigRepository.Update(menuConfig);
                await _unitOfWork.CompleteAsync();
                
                if (startedTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
                return true;
            }
            catch (Exception)
            {
                if (startedTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }


        private async Task<bool> IsChildOf(Guid menuId, Guid potentialChildId)
        {
            var menuConfig = await _menuConfigRepository.GetByIdAsync(menuId);
            if (menuConfig == null || menuConfig.IsDeleted || !menuConfig.ParentId.HasValue)
            {
                return false;
            }
            
            if (menuConfig.ParentId.Value == potentialChildId)
            {
                return true;
            }
            
            return await IsChildOf(menuConfig.ParentId.Value, potentialChildId);
        }

        public async Task<List<MenuConfigDto>> GetMenuTreeAsync()
        {

            var allMenus = await _menuConfigRepository.GetAllAsync();
            var menuDtos = _mapper.Map<List<MenuConfigDto>>(allMenus.Where(m => !m.IsDeleted).ToList());
            

            var menuDict = new Dictionary<Guid, MenuConfigDto>();
            foreach (var menu in menuDtos)
            {
                menuDict[menu.Id] = menu;
                menu.Children = new List<MenuConfigDto>(); 
            }
            

            var rootMenus = new List<MenuConfigDto>();
            

            foreach (var menu in menuDtos)
            {
                if (menu.ParentId == null)
                {

                    rootMenus.Add(menu);
                }
                else if (menuDict.ContainsKey(menu.ParentId.Value))
                {

                    menuDict[menu.ParentId.Value].Children.Add(menu);
                }
            }
            

            rootMenus = rootMenus.OrderBy(m => m.DisplayOrder).ToList();
            

            foreach (var rootMenu in rootMenus)
            {
                SortChildMenus(rootMenu);
            }
            
            return rootMenus;
        }

        private void SortChildMenus(MenuConfigDto menu)
        {

            menu.Children = menu.Children.OrderBy(m => m.DisplayOrder).ToList();
            

            foreach (var childMenu in menu.Children)
            {
                SortChildMenus(childMenu);
            }
        }
    }
} 
