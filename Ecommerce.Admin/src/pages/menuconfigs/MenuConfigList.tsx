import React, { useState, useEffect, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { FiPlus, FiEdit, FiTrash2, FiChevronDown, FiChevronRight, FiEye, FiEyeOff } from 'react-icons/fi';
import { menuConfigsApiService } from '../../services/api';
import { categoriesApiService } from '../../services/api';
import { MenuConfig } from '../../models/MenuConfig';
import { Category } from '../../models/Category';
import Swal from 'sweetalert2';

// Component hiển thị một nút trong cây menu
interface MenuNodeProps {
  menu: MenuConfig;
  level: number;
  onAddChild: (parentId: string, parentName: string) => void;
  onEdit: (menuId: string) => void;
  onDelete: (menuId: string, menuName: string) => void;
  onExpand: (menuId: string) => void;
  expandedMenus: Record<string, boolean>;
}

const MenuNode: React.FC<MenuNodeProps> = ({ 
  menu, 
  level, 
  onAddChild, 
  onEdit, 
  onDelete,
  onExpand,
  expandedMenus
}) => {
  const hasChildren = menu.children && menu.children.length > 0;
  const isExpanded = expandedMenus[menu.id] || false;
  
  // CSS cho các cấp độ menu
  const levelStyles = {
    padding: `${8 + level * 16}px 16px 8px ${32 + level * 24}px`,
    position: 'relative' as 'relative',
  };

  // CSS cho đường kết nối
  const lineStyles = level > 0 ? {
    content: '""',
    position: 'absolute' as 'absolute',
    left: `${16 + (level - 1) * 24}px`,
    top: '0',
    borderLeft: '1px dashed #d1d5db',
    height: '100%'
  } : {};

  // CSS cho nhánh ngang
  const branchStyles = level > 0 ? {
    content: '""',
    position: 'absolute' as 'absolute',
    left: `${16 + (level - 1) * 24}px`,
    top: '20px',
    width: '16px',
    borderTop: '1px dashed #d1d5db'
  } : {};

  return (
    <div className="menu-node">
      <div 
        className={`relative flex items-center mb-1 p-3 rounded-lg border ${menu.isMainMenu ? 'border-indigo-200 bg-indigo-50' : 'border-purple-200 bg-purple-50'}`}
        style={levelStyles}
      >
        {level > 0 && <div style={lineStyles}></div>}
        {level > 0 && <div style={branchStyles}></div>}
        
        {/* Nút mở rộng */}
        {hasChildren && (
          <button 
            onClick={() => onExpand(menu.id)}
            className="mr-2 p-1 rounded-full hover:bg-white focus:outline-none transition-colors duration-150"
          >
            {isExpanded ? 
              <FiChevronDown className={menu.isMainMenu ? "text-indigo-600" : "text-purple-600"} /> : 
              <FiChevronRight className={menu.isMainMenu ? "text-indigo-600" : "text-purple-600"} />}
          </button>
        )}

        {/* Icon hiển thị */}
        {menu.icon ? (
          <i className={`${menu.icon} mx-2 ${menu.isMainMenu ? "text-indigo-500" : "text-purple-500"}`}></i>
        ) : (
          <div className="w-6 h-6 mx-2 flex items-center justify-center rounded-full bg-white">
            {menu.isMainMenu ? (
              <span className="text-indigo-500 text-xs font-bold">M</span>
            ) : (
              <span className="text-purple-500 text-xs font-bold">S</span>
            )}
          </div>
        )}
        
        {/* Thông tin menu */}
        <div className="flex-1 ml-2">
          <div className="flex items-center">
            <h3 className={`font-medium ${menu.isMainMenu ? "text-indigo-700" : "text-purple-700"}`}>
              {menu.customName || menu.categoryName}
            </h3>
            <span className={`ml-2 text-xs px-2 py-1 rounded-full ${menu.isMainMenu ? "bg-indigo-100 text-indigo-700" : "bg-purple-100 text-purple-700"}`}>
              {menu.isMainMenu ? 'Menu chính' : 'Menu phụ'}
            </span>
            <span className={`ml-2 text-xs px-2 py-1 rounded-full ${menu.isVisible ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-700"}`}>
              {menu.isVisible ? <FiEye className="inline mr-1" /> : <FiEyeOff className="inline mr-1" />}
              {menu.isVisible ? 'Hiển thị' : 'Ẩn'}
            </span>
          </div>
          <div className="text-sm text-gray-500">
            {hasChildren && (
              <span className="mr-3">
                {menu.children.length} menu con
              </span>
            )}
            <span className="mr-3">Thứ tự: {menu.displayOrder}</span>
          </div>
        </div>
        
        {/* Các nút thao tác */}
        <div className="flex space-x-2">
          <button
            onClick={() => onAddChild(menu.id, menu.customName || menu.categoryName)}
            className={`p-1.5 rounded-full ${menu.isMainMenu ? "bg-indigo-100 text-indigo-600 hover:bg-indigo-200" : "bg-purple-100 text-purple-600 hover:bg-purple-200"}`}
            title="Thêm menu con"
          >
            <FiPlus size={16} />
          </button>
          <button
            onClick={() => onEdit(menu.id)}
            className="p-1.5 rounded-full bg-blue-100 text-blue-600 hover:bg-blue-200"
            title="Sửa menu"
          >
            <FiEdit size={16} />
          </button>
          <button
            onClick={() => onDelete(menu.id, menu.customName || menu.categoryName)}
            className="p-1.5 rounded-full bg-red-100 text-red-600 hover:bg-red-200"
            title="Xóa menu"
          >
            <FiTrash2 size={16} />
          </button>
        </div>
      </div>
      
      {/* Hiển thị menu con */}
      {hasChildren && isExpanded && (
        <div className="menu-children">
          {menu.children.map((child) => (
            <MenuNode
              key={child.id}
              menu={child}
              level={level + 1}
              onAddChild={onAddChild}
              onEdit={onEdit}
              onDelete={onDelete}
              onExpand={onExpand}
              expandedMenus={expandedMenus}
            />
          ))}
        </div>
      )}
    </div>
  );
};

const MenuConfigList: React.FC = () => {
  const navigate = useNavigate();
  const [menuTree, setMenuTree] = useState<MenuConfig[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [expandedMenus, setExpandedMenus] = useState<Record<string, boolean>>({});
  const [menuType, setMenuType] = useState<'all' | 'main' | 'sub'>('all');

  const fetchMenuTree = useCallback(async () => {
    setIsLoading(true);
    try {
      // Lấy cấu trúc cây menu từ endpoint mới
      const tree = await menuConfigsApiService.getMenuTree();
      
      // Lọc theo loại menu nếu cần
      let filteredTree = [...tree];
      if (menuType === 'main') {
        filteredTree = tree.filter(menu => menu.isMainMenu);
      } else if (menuType === 'sub') {
        filteredTree = tree.filter(menu => !menu.isMainMenu);
      }
      
      setMenuTree(filteredTree);
      
      // Lấy danh sách các danh mục để hiển thị thông tin
      const allCategories = await categoriesApiService.getAll();
      setCategories(allCategories);
      
      // Tự động mở rộng tất cả menu gốc
      const expandedMap: Record<string, boolean> = {};
      filteredTree.forEach(menu => {
        expandedMap[menu.id] = true;
      });
      setExpandedMenus(expandedMap);
    } catch (error) {
      console.error('Lỗi khi lấy cấu trúc cây menu:', error);
      Swal.fire({
        title: 'Lỗi!',
        text: 'Không thể tải dữ liệu cấu hình menu.',
        icon: 'error',
        confirmButtonText: 'OK'
      });
    } finally {
      setIsLoading(false);
    }
  }, [menuType]);

  useEffect(() => {
    fetchMenuTree();
  }, [fetchMenuTree]);

  const handleAddChildMenu = (parentId: string, parentName: string) => {
    // Lưu ID của menu cha vào localStorage để form tạo mới có thể sử dụng
    localStorage.setItem('parentMenuId', parentId);
    localStorage.setItem('parentMenuName', parentName);
    navigate('/menu-configs/create');
  };

  const handleEditMenu = (menuId: string) => {
    navigate(`/menu-configs/edit/${menuId}`);
  };

  const handleDeleteMenu = (menuId: string, menuName: string) => {
    Swal.fire({
      title: 'Xác nhận xóa?',
      text: `Bạn có chắc chắn muốn xóa menu "${menuName}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    }).then(async (result) => {
      if (result.isConfirmed) {
        try {
          await menuConfigsApiService.delete(menuId);
          Swal.fire(
            'Đã xóa!',
            'Menu đã được xóa thành công.',
            'success'
          );
          fetchMenuTree();
        } catch (error) {
          console.error('Lỗi khi xóa menu:', error);
          Swal.fire(
            'Lỗi!',
            'Có lỗi xảy ra khi xóa menu. Menu này có thể có menu con.',
            'error'
          );
        }
      }
    });
  };

  const toggleExpandMenu = (menuId: string) => {
    setExpandedMenus(prev => ({
      ...prev,
      [menuId]: !prev[menuId]
    }));
  };

  const expandAllMenus = () => {
    const expandAll: Record<string, boolean> = {};
    
    const setExpandedForMenusRecursively = (menus: MenuConfig[]) => {
      menus.forEach(menu => {
        expandAll[menu.id] = true;
        if (menu.children && menu.children.length > 0) {
          setExpandedForMenusRecursively(menu.children);
        }
      });
    };
    
    setExpandedForMenusRecursively(menuTree);
    setExpandedMenus(expandAll);
  };

  const collapseAllMenus = () => {
    // Chỉ để lại trạng thái mở rộng cho các menu gốc
    const collapseAll: Record<string, boolean> = {};
    menuTree.forEach(menu => {
      collapseAll[menu.id] = true;
    });
    setExpandedMenus(collapseAll);
  };

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Cấu trúc menu</h1>
          <p className="text-sm text-gray-600 mt-1">
            Quản lý và sắp xếp menu hiển thị trên website
          </p>
        </div>
        <div className="flex space-x-4">
          {/* Bộ lọc loại menu */}
          <div className="flex items-center shadow-sm rounded-md overflow-hidden">
            <button
              onClick={() => setMenuType('all')}
              className={`px-4 py-2 font-medium text-sm focus:outline-none ${
                menuType === 'all'
                  ? 'bg-gray-600 text-white'
                  : 'bg-white text-gray-700 hover:bg-gray-50'
              }`}
            >
              Tất cả
            </button>
            <button
              onClick={() => setMenuType('main')}
              className={`px-4 py-2 font-medium text-sm focus:outline-none ${
                menuType === 'main'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white text-gray-700 hover:bg-gray-50'
              }`}
            >
              Menu chính
            </button>
            <button
              onClick={() => setMenuType('sub')}
              className={`px-4 py-2 font-medium text-sm focus:outline-none ${
                menuType === 'sub'
                  ? 'bg-purple-600 text-white'
                  : 'bg-white text-gray-700 hover:bg-gray-50'
              }`}
            >
              Menu phụ
            </button>
          </div>
          
          {/* Nút thêm menu gốc */}
          <Link
            to="/menu-configs/create"
            className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 px-4 rounded flex items-center transition-colors duration-150"
          >
            <FiPlus className="mr-2" /> Thêm menu gốc
          </Link>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500"></div>
        </div>
      ) : menuTree.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm p-8 text-center">
          <div className="mx-auto w-16 h-16 mb-4 text-gray-300">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h7" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Chưa có cấu hình menu</h3>
          <p className="text-gray-500 mb-6">Bắt đầu bằng cách tạo menu gốc đầu tiên</p>
          <Link
            to="/menu-configs/create"
            className="bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 px-4 rounded inline-flex items-center transition-colors duration-150"
          >
            <FiPlus className="mr-2" /> Tạo menu gốc
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-sm p-6">
          <div className="menu-tree">
            {menuTree.map((menu) => (
              <MenuNode
                key={menu.id}
                menu={menu}
                level={0}
                onAddChild={handleAddChildMenu}
                onEdit={handleEditMenu}
                onDelete={handleDeleteMenu}
                onExpand={toggleExpandMenu}
                expandedMenus={expandedMenus}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default MenuConfigList; 