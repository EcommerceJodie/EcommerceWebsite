import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { FiSave, FiX, FiUpload, FiTrash2, FiStar } from 'react-icons/fi';
import { productsApiService } from '../../services/api/products.service';
import { categoriesApiService } from '../../services/api/categories.service';
import { Product } from '../../models/Product';

interface ProductFormProps {
  isEdit?: boolean;
}

const ProductForm = ({ isEdit = false }: ProductFormProps) => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [categories, setCategories] = useState<{ id: string; categoryName: string }[]>([]);
  
  const [formData, setFormData] = useState<{
    productName: string;
    productDescription: string;
    productSlug: string;
    productPrice: string;
    productDiscountPrice: string;
    productStock: string;
    productSku: string;
    productStatus: string;
    isFeatured: boolean;
    categoryId: string;
    metaTitle: string;
    metaDescription: string;
  }>({
    productName: '',
    productDescription: '',
    productSlug: '',
    productPrice: '',
    productDiscountPrice: '',
    productStock: '',
    productSku: '',
    productStatus: 'Active',
    isFeatured: false,
    categoryId: '',
    metaTitle: '',
    metaDescription: ''
  });
  
  const [productImages, setProductImages] = useState<{
    id?: string;
    imageUrl: string;
    isMainImage: boolean;
    file?: File;
    isNew?: boolean;
    isDeleted?: boolean;
  }[]>([]);
  
  const [mainImage, setMainImage] = useState<File | null>(null);
  const [mainImagePreview, setMainImagePreview] = useState<string | null>(null);
  const [additionalImages, setAdditionalImages] = useState<File[]>([]);
  const [imagesPreviews, setImagesPreviews] = useState<string[]>([]);
  
  // Tải danh sách danh mục
  const fetchCategories = useCallback(async () => {
    try {
      const data = await categoriesApiService.getAll();
      setCategories(data);
    } catch (err) {
      console.error('Error fetching categories:', err);
      setError('Không thể tải danh sách danh mục. Vui lòng thử lại sau.');
    }
  }, []);
  
  // Tải thông tin sản phẩm nếu đang ở chế độ chỉnh sửa
  const fetchProductData = useCallback(async () => {
    if (!isEdit || !id) return;
    
    setIsLoading(true);
    setError(null);
    
    try {
      const product = await productsApiService.getById(id);
      
      setFormData({
        productName: product.productName,
        productDescription: product.productDescription,
        productSlug: product.productSlug || '',
        productPrice: product.productPrice.toString(),
        productDiscountPrice: product.productDiscountPrice?.toString() || '',
        productStock: product.productStock.toString(),
        productSku: product.productSku,
        productStatus: product.productStatus,
        isFeatured: product.isFeatured,
        categoryId: product.categoryId,
        metaTitle: product.metaTitle || '',
        metaDescription: product.metaDescription || ''
      });
      
      if (product.imageUrls && product.imageUrls.length > 0) {
        // Giả định: imageUrls chứa các URL của hình ảnh
        // Trong dự án thực tế, cần API để lấy thông tin chi tiết về hình ảnh
        const images = product.imageUrls.map((url, index) => ({
          id: `img-${index}`, // Ideally, this should be the actual image ID from the backend
          imageUrl: url,
          isMainImage: index === 0, // Giả định hình đầu tiên là hình chính
          isNew: false,
          isDeleted: false
        }));
        
        setProductImages(images);
      }
    } catch (err) {
      console.error('Error fetching product:', err);
      setError('Không thể tải thông tin sản phẩm. Vui lòng thử lại sau.');
    } finally {
      setIsLoading(false);
    }
  }, [isEdit, id]);
  
  useEffect(() => {
    fetchCategories();
    fetchProductData();
  }, [fetchCategories, fetchProductData]);
  
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };
  
  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData(prev => ({ ...prev, [name]: checked }));
  };
  
  const handleMainImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setMainImage(file);
      
      // Tạo preview cho hình ảnh
      const reader = new FileReader();
      reader.onload = () => {
        setMainImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };
  
  const handleAdditionalImagesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      setAdditionalImages(prev => [...prev, ...files]);
      
      // Tạo previews cho hình ảnh
      files.forEach(file => {
        const reader = new FileReader();
        reader.onload = () => {
          setImagesPreviews(prev => [...prev, reader.result as string]);
        };
        reader.readAsDataURL(file);
      });
    }
  };
  
  const handleRemoveImage = (index: number) => {
    setImagesPreviews(prev => prev.filter((_, i) => i !== index));
    setAdditionalImages(prev => prev.filter((_, i) => i !== index));
  };
  
  const handleRemoveMainImage = () => {
    setMainImage(null);
    setMainImagePreview(null);
  };
  
  const handleSetAsMainImage = (index: number) => {
    if (isEdit && productImages.length > 0) {
      const updatedImages = [...productImages];
      updatedImages.forEach((img, i) => {
        img.isMainImage = i === index;
      });
      setProductImages(updatedImages);
    }
  };
  
  const handleRemoveExistingImage = (index: number) => {
    const updatedImages = [...productImages];
    updatedImages[index].isDeleted = true;
    setProductImages(updatedImages);
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSaving(true);
    
    try {
      // Kiểm tra dữ liệu trước khi gửi
      if (!formData.productName || !formData.productPrice || !formData.categoryId) {
        throw new Error('Vui lòng điền đầy đủ thông tin bắt buộc: Tên sản phẩm, Giá, Danh mục');
      }
      
      // Tạo FormData object để gửi cả dữ liệu và hình ảnh
      const data = new FormData();
      
      // Thêm thông tin sản phẩm
      Object.entries(formData).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          data.append(key, value.toString());
        }
      });
      
      // Thêm hình ảnh chính
      if (mainImage) {
        data.append('mainImage', mainImage);
      }
      
      // Thêm các hình ảnh bổ sung
      additionalImages.forEach(file => {
        data.append('productImages', file);
      });
      
      let response;
      
      if (isEdit && id) {
        // Cập nhật sản phẩm
        response = await productsApiService.update(id, data);
        
        // Nếu có hình ảnh cần xóa
        const imagesToDelete = productImages.filter(img => img.isDeleted && img.id);
        for (const img of imagesToDelete) {
          if (img.id) {
            await productsApiService.deleteProductImage(img.id);
          }
        }
        
        // Nếu cần thay đổi hình ảnh chính
        const newMainImage = productImages.find(img => img.isMainImage && !img.isDeleted && img.id);
        if (newMainImage && newMainImage.id) {
          await productsApiService.setMainImage(newMainImage.id);
        }
      } else {
        // Tạo sản phẩm mới
        response = await productsApiService.create(data);
      }
      
      // Quay lại trang danh sách sản phẩm
      navigate('/products');
    } catch (err) {
      console.error('Error saving product:', err);
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Đã xảy ra lỗi khi lưu sản phẩm. Vui lòng thử lại sau.');
      }
    } finally {
      setIsSaving(false);
    }
  };
  
  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
      </div>
    );
  }
  
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">
          {isEdit ? 'Chỉnh sửa sản phẩm' : 'Thêm sản phẩm mới'}
        </h1>
        <div className="flex space-x-2">
          <button
            onClick={() => navigate('/products')}
            className="bg-gray-200 hover:bg-gray-300 text-gray-700 py-2 px-4 rounded-md inline-flex items-center transition-colors duration-300"
          >
            <FiX className="mr-2" /> Hủy
          </button>
          <button
            onClick={handleSubmit}
            disabled={isSaving}
            className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-md inline-flex items-center transition-colors duration-300"
          >
            <FiSave className="mr-2" /> {isSaving ? 'Đang lưu...' : 'Lưu'}
          </button>
        </div>
      </div>
      
      {error && (
        <div className="mb-6 bg-red-50 text-red-600 p-4 rounded-md">
          {error}
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Thông tin cơ bản */}
          <div className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tên sản phẩm <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                name="productName"
                value={formData.productName}
                onChange={handleInputChange}
                className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                required
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Slug (URL)
              </label>
              <input
                type="text"
                name="productSlug"
                value={formData.productSlug}
                onChange={handleInputChange}
                className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
              <p className="mt-1 text-xs text-gray-500">
                Để trống để tự động tạo từ tên sản phẩm
              </p>
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mô tả sản phẩm
              </label>
              <textarea
                name="productDescription"
                value={formData.productDescription}
                onChange={handleInputChange}
                rows={5}
                className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Giá <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  name="productPrice"
                  value={formData.productPrice}
                  onChange={handleInputChange}
                  min="0"
                  step="1000"
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Giá khuyến mãi
                </label>
                <input
                  type="number"
                  name="productDiscountPrice"
                  value={formData.productDiscountPrice}
                  onChange={handleInputChange}
                  min="0"
                  step="1000"
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Số lượng tồn kho <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  name="productStock"
                  value={formData.productStock}
                  onChange={handleInputChange}
                  min="0"
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Mã SKU <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="productSku"
                  value={formData.productSku}
                  onChange={handleInputChange}
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Danh mục <span className="text-red-500">*</span>
                </label>
                <select
                  name="categoryId"
                  value={formData.categoryId}
                  onChange={handleInputChange}
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  required
                >
                  <option value="">Chọn danh mục</option>
                  {categories.map(category => (
                    <option key={category.id} value={category.id}>
                      {category.categoryName}
                    </option>
                  ))}
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Trạng thái
                </label>
                <select
                  name="productStatus"
                  value={formData.productStatus}
                  onChange={handleInputChange}
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="Active">Đang hoạt động</option>
                  <option value="Inactive">Ngừng hoạt động</option>
                  <option value="Draft">Nháp</option>
                </select>
              </div>
            </div>
            
            <div>
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isFeatured"
                  name="isFeatured"
                  checked={formData.isFeatured}
                  onChange={handleCheckboxChange}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="isFeatured" className="ml-2 block text-sm text-gray-700">
                  Đánh dấu là sản phẩm nổi bật
                </label>
              </div>
            </div>
          </div>
          
          {/* Hình ảnh và thông tin SEO */}
          <div className="space-y-6">
            <div>
              <h3 className="text-lg font-medium text-gray-700 mb-2">Hình ảnh sản phẩm</h3>
              
              {/* Hình ảnh chính */}
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hình ảnh chính
                </label>
                
                <div className="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-md relative">
                  {mainImagePreview ? (
                    <div className="relative">
                      <img src={mainImagePreview} alt="Preview" className="h-40 object-contain" />
                      <button
                        type="button"
                        onClick={handleRemoveMainImage}
                        className="absolute top-0 right-0 bg-red-500 text-white rounded-full p-1 shadow-lg hover:bg-red-600 transition-colors"
                      >
                        <FiTrash2 size={16} />
                      </button>
                    </div>
                  ) : (
                    <div className="space-y-1 text-center">
                      <FiUpload className="mx-auto h-12 w-12 text-gray-400" />
                      <div className="flex text-sm text-gray-600">
                        <label htmlFor="main-image" className="relative cursor-pointer bg-white rounded-md font-medium text-blue-600 hover:text-blue-500 focus-within:outline-none">
                          <span>Tải ảnh lên</span>
                          <input id="main-image" name="main-image" type="file" className="sr-only" onChange={handleMainImageChange} accept="image/*" />
                        </label>
                        <p className="pl-1">hoặc kéo thả vào đây</p>
                      </div>
                      <p className="text-xs text-gray-500">PNG, JPG, GIF tối đa 5MB</p>
                    </div>
                  )}
                </div>
              </div>
              
              {/* Hiển thị hình ảnh đã có nếu đang chỉnh sửa */}
              {isEdit && productImages.length > 0 && (
                <div className="mb-4">
                  <h4 className="text-sm font-medium text-gray-700 mb-2">Hình ảnh hiện tại</h4>
                  <div className="grid grid-cols-3 gap-4">
                    {productImages
                      .filter(img => !img.isDeleted)
                      .map((image, idx) => (
                        <div key={idx} className="relative">
                          <img src={image.imageUrl} alt={`Product ${idx}`} className="h-24 w-full object-cover rounded-md" />
                          <div className="absolute top-0 right-0 flex space-x-1">
                            <button
                              type="button"
                              onClick={() => handleSetAsMainImage(idx)}
                              className={`p-1 rounded-full shadow-lg hover:bg-yellow-400 transition-colors ${
                                image.isMainImage ? 'bg-yellow-500 text-white' : 'bg-gray-200 text-gray-600'
                              }`}
                            >
                              <FiStar size={16} />
                            </button>
                            <button
                              type="button"
                              onClick={() => handleRemoveExistingImage(idx)}
                              className="bg-red-500 text-white rounded-full p-1 shadow-lg hover:bg-red-600 transition-colors"
                            >
                              <FiTrash2 size={16} />
                            </button>
                          </div>
                          {image.isMainImage && (
                            <div className="absolute bottom-0 left-0 right-0 bg-yellow-500 text-white text-xs text-center py-1">
                              Hình chính
                            </div>
                          )}
                        </div>
                      ))}
                  </div>
                </div>
              )}
              
              {/* Hình ảnh bổ sung */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hình ảnh bổ sung
                </label>
                
                <div className="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-md">
                  <div className="space-y-1 text-center">
                    <FiUpload className="mx-auto h-12 w-12 text-gray-400" />
                    <div className="flex text-sm text-gray-600">
                      <label htmlFor="additional-images" className="relative cursor-pointer bg-white rounded-md font-medium text-blue-600 hover:text-blue-500 focus-within:outline-none">
                        <span>Tải ảnh lên</span>
                        <input id="additional-images" name="additional-images" type="file" className="sr-only" onChange={handleAdditionalImagesChange} accept="image/*" multiple />
                      </label>
                      <p className="pl-1">hoặc kéo thả vào đây</p>
                    </div>
                    <p className="text-xs text-gray-500">PNG, JPG, GIF tối đa 5MB</p>
                  </div>
                </div>
                
                {/* Preview hình ảnh bổ sung */}
                {imagesPreviews.length > 0 && (
                  <div className="mt-4 grid grid-cols-3 gap-4">
                    {imagesPreviews.map((preview, index) => (
                      <div key={index} className="relative">
                        <img src={preview} alt={`Preview ${index}`} className="h-24 w-full object-cover rounded-md" />
                        <button
                          type="button"
                          onClick={() => handleRemoveImage(index)}
                          className="absolute top-0 right-0 bg-red-500 text-white rounded-full p-1 shadow-lg hover:bg-red-600 transition-colors"
                        >
                          <FiTrash2 size={16} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
            
            {/* SEO Information */}
            <div className="mt-6">
              <h3 className="text-lg font-medium text-gray-700 mb-2">Thông tin SEO</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Meta Title
                  </label>
                  <input
                    type="text"
                    name="metaTitle"
                    value={formData.metaTitle}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Meta Description
                  </label>
                  <textarea
                    name="metaDescription"
                    value={formData.metaDescription}
                    onChange={handleInputChange}
                    rows={3}
                    className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  );
};

export default ProductForm; 