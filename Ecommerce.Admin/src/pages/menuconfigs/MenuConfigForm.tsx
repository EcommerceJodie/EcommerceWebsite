import React, { useState, useEffect, ChangeEvent, FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { menuConfigsApiService } from '../../services/api';
import { categoriesApiService } from '../../services/api';
import { MenuConfig, CreateMenuConfigDto, UpdateMenuConfigDto } from '../../models/MenuConfig';
import { Category } from '../../models/Category';
import { FiSave, FiX } from 'react-icons/fi';
import Swal from 'sweetalert2';

interface MenuConfigFormProps {
  isEdit?: boolean;
}

const MenuConfigForm: React.FC<MenuConfigFormProps> = ({ isEdit = false }) => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [formData, setFormData] = useState<CreateMenuConfigDto>({
    categoryId: '',
    parentId: '',
    customName: '',
    icon: '',
    displayOrder: 0,
    isVisible: true,
    isMainMenu: true
  });

  const [categories, setCategories] = useState<Category[]>([]);
  const [rootMenuConfigs, setRootMenuConfigs] = useState<MenuConfig[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      try {
        const allCategories = await categoriesApiService.getAll();
        setCategories(allCategories);

        // Lấy tất cả menu gốc cho dropdown
        const rootMenus = await menuConfigsApiService.getRootMenuConfigs();
        setRootMenuConfigs(rootMenus);

        // Nếu đang ở chế độ chỉnh sửa
        if (id) {
          const menuConfig = await menuConfigsApiService.getById(id);
          setFormData({
            categoryId: menuConfig.categoryId,
            parentId: menuConfig.parentId || '',
            customName: menuConfig.customName || '',
            icon: menuConfig.icon || '',
            displayOrder: menuConfig.displayOrder,
            isVisible: menuConfig.isVisible,
            isMainMenu: menuConfig.isMainMenu
          });
        } 
        // Nếu đang ở chế độ tạo menu con
        else {
          const parentMenuId = localStorage.getItem('parentMenuId');
          const parentMenuName = localStorage.getItem('parentMenuName');
          
          if (parentMenuId) {
            // Tự động điền giá trị menu cha
            setFormData(prevState => ({
              ...prevState,
              parentId: parentMenuId,
              isMainMenu: false // Menu con không thể là menu chính
            }));
            
            // Hiển thị thông báo nhỏ về việc đang tạo menu con
            Swal.fire({
              title: 'Tạo menu con',
              text: `Bạn đang tạo menu con cho "${parentMenuName}"`,
              icon: 'info',
              timer: 3000,
              showConfirmButton: false
            });
            
            // Xóa thông tin menu cha khỏi localStorage sau khi sử dụng
            localStorage.removeItem('parentMenuId');
            localStorage.removeItem('parentMenuName');
          }
        }
      } catch (error) {
        console.error('Error fetching data:', error);
        Swal.fire({
          title: 'Lỗi!',
          text: 'Không thể tải dữ liệu.',
          icon: 'error'
        });
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [id]);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.categoryId) {
      newErrors.categoryId = 'Vui lòng chọn danh mục.';
    }

    if (formData.displayOrder < 0) {
      newErrors.displayOrder = 'Thứ tự hiển thị không được là số âm.';
    }

    if (formData.customName && formData.customName.length > 100) {
      newErrors.customName = 'Tên tùy chỉnh không được vượt quá 100 ký tự.';
    }

    if (formData.icon && formData.icon.length > 50) {
      newErrors.icon = 'Icon không được vượt quá 50 ký tự.';
    }

    // Đảm bảo không chọn chính nó làm parent
    if (isEdit && formData.parentId === id) {
      newErrors.parentId = 'Không thể chọn chính mình làm menu cha.';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    
    if (type === 'checkbox') {
      const isChecked = (e.target as HTMLInputElement).checked;
      setFormData(prev => ({ ...prev, [name]: isChecked }));
    } else if (name === 'parentId' && value === '') {
      // Xử lý trường hợp chọn "Không có menu cha"
      setFormData(prev => ({ ...prev, [name]: null }));
    } else {
      setFormData(prev => ({ ...prev, [name]: value }));
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);

    try {
      if (isEdit && id) {
        await menuConfigsApiService.update(id, formData as UpdateMenuConfigDto);
        Swal.fire({
          title: 'Thành công!',
          text: 'Cập nhật cấu hình menu thành công.',
          icon: 'success',
          confirmButtonText: 'OK'
        });
      } else {
        await menuConfigsApiService.create(formData as CreateMenuConfigDto);
        Swal.fire({
          title: 'Thành công!',
          text: 'Thêm cấu hình menu mới thành công.',
          icon: 'success',
          confirmButtonText: 'OK'
        });
      }
      navigate('/menu-configs');
    } catch (error: any) {
      Swal.fire({
        title: 'Lỗi!',
        text: error.message || 'Có lỗi xảy ra khi lưu cấu hình menu.',
        icon: 'error',
        confirmButtonText: 'OK'
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="loader">Đang tải...</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEdit ? 'Sửa cấu hình menu' : 'Thêm cấu hình menu mới'}
        </h1>
      </div>

      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <form onSubmit={handleSubmit} className="px-8 py-6">
          <div className="grid grid-cols-1 gap-y-6 gap-x-8 sm:grid-cols-2">
            <div>
              <label htmlFor="categoryId" className="block text-sm font-medium text-gray-700">
                Danh mục <span className="text-red-500">*</span>
              </label>
              <select
                id="categoryId"
                name="categoryId"
                value={formData.categoryId}
                onChange={handleInputChange}
                className={`mt-1 block w-full py-2 px-3 border ${
                  errors.categoryId ? 'border-red-300' : 'border-gray-300'
                } bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
              >
                <option value="">Chọn danh mục</option>
                {categories.map(category => (
                  <option key={category.id} value={category.id}>
                    {category.categoryName}
                  </option>
                ))}
              </select>
              {errors.categoryId && (
                <p className="mt-2 text-sm text-red-600">{errors.categoryId}</p>
              )}
            </div>

            <div className="mb-4">
              <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="parentId">
                Menu cha
              </label>
              <select
                id="parentId"
                name="parentId"
                disabled={formData.isMainMenu} // Vô hiệu hóa nếu là menu chính
                className={`shadow appearance-none border ${
                  errors.parentId ? 'border-red-500' : 'border-gray-300'
                } rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline ${
                  formData.isMainMenu ? 'bg-gray-100' : ''
                }`}
                value={formData.parentId}
                onChange={handleInputChange}
              >
                <option value="">-- Chọn menu cha --</option>
                {rootMenuConfigs.map((menu) => (
                  <option key={menu.id} value={menu.id}>
                    {menu.customName || menu.categoryName}
                  </option>
                ))}
              </select>
              {errors.parentId && (
                <p className="text-red-500 text-xs italic">{errors.parentId}</p>
              )}
              {!formData.isMainMenu && !formData.parentId && (
                <p className="text-yellow-500 text-xs mt-1">
                  <i className="fas fa-exclamation-triangle mr-1"></i>
                  Menu phụ cần phải chọn một menu cha
                </p>
              )}
            </div>

            <div>
              <label htmlFor="customName" className="block text-sm font-medium text-gray-700">
                Tên tùy chỉnh
              </label>
              <input
                type="text"
                id="customName"
                name="customName"
                value={formData.customName}
                onChange={handleInputChange}
                className={`mt-1 block w-full py-2 px-3 border ${
                  errors.customName ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                placeholder="Nhập tên tùy chỉnh"
              />
              {errors.customName && (
                <p className="mt-2 text-sm text-red-600">{errors.customName}</p>
              )}
            </div>

            <div>
              <label htmlFor="icon" className="block text-sm font-medium text-gray-700">
                Icon
              </label>
              <input
                type="text"
                id="icon"
                name="icon"
                value={formData.icon}
                onChange={handleInputChange}
                className={`mt-1 block w-full py-2 px-3 border ${
                  errors.icon ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                placeholder="bi bi-house (Bootstrap Icons)"
              />
              {errors.icon && (
                <p className="mt-2 text-sm text-red-600">{errors.icon}</p>
              )}
              <p className="mt-1 text-sm text-gray-500">
                Sử dụng tên class của Bootstrap Icons (mặc định), ví dụ: bi bi-house
              </p>
            </div>

            <div>
              <label htmlFor="displayOrder" className="block text-sm font-medium text-gray-700">
                Thứ tự hiển thị
              </label>
              <input
                type="number"
                id="displayOrder"
                name="displayOrder"
                value={formData.displayOrder}
                onChange={handleInputChange}
                className={`mt-1 block w-full py-2 px-3 border ${
                  errors.displayOrder ? 'border-red-300' : 'border-gray-300'
                } rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                min="0"
              />
              {errors.displayOrder && (
                <p className="mt-2 text-sm text-red-600">{errors.displayOrder}</p>
              )}
            </div>

            <div className="sm:col-span-2">
              <div className="flex items-start">
                <div className="flex items-center h-5">
                  <input
                    id="isVisible"
                    name="isVisible"
                    type="checkbox"
                    checked={formData.isVisible}
                    onChange={handleInputChange}
                    className="focus:ring-indigo-500 h-4 w-4 text-indigo-600 border-gray-300 rounded"
                  />
                </div>
                <div className="ml-3 text-sm">
                  <label htmlFor="isVisible" className="font-medium text-gray-700">
                    Hiển thị
                  </label>
                  <p className="text-gray-500">Menu sẽ được hiển thị trên trang người dùng.</p>
                </div>
              </div>
            </div>

            <div className="sm:col-span-2">
              <div className="flex items-start">
                <div className="flex items-center h-5">
                  <input
                    id="isMainMenu"
                    name="isMainMenu"
                    type="checkbox"
                    checked={formData.isMainMenu}
                    onChange={handleInputChange}
                    className="focus:ring-indigo-500 h-4 w-4 text-indigo-600 border-gray-300 rounded"
                  />
                </div>
                <div className="ml-3 text-sm">
                  <label htmlFor="isMainMenu" className="font-medium text-gray-700">
                    Menu chính
                  </label>
                  <p className="text-gray-500">
                    Menu sẽ được hiển thị ở khu vực menu chính. Nếu không chọn, menu sẽ được hiển thị ở khu vực menu phụ.
                  </p>
                </div>
              </div>
            </div>
          </div>

          <div className="mt-8 flex justify-end space-x-3">
            <button
              type="button"
              onClick={() => navigate('/menu-configs')}
              className="inline-flex justify-center py-2 px-4 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              <FiX className="mr-2 h-5 w-5" />
              Hủy
            </button>
            <button
              type="submit"
              className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              disabled={isLoading}
            >
              <FiSave className="mr-2 h-5 w-5" />
              {isLoading ? 'Đang lưu...' : 'Lưu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default MenuConfigForm; 