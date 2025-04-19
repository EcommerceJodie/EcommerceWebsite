import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { FiPlus, FiEdit, FiTrash2, FiSearch, FiFilter } from 'react-icons/fi';
import { productsApiService, ProductQueryParams, PagedResult } from '../../services/api/products.service';
import { categoriesApiService } from '../../services/api/categories.service';
import { Product } from '../../models/Product';

interface Category {
  id: string;
  categoryName: string;
}

const ProductList = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [queryParams, setQueryParams] = useState<ProductQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDesc: true
  });

  // Lấy danh sách danh mục
  const fetchCategories = useCallback(async () => {
    try {
      const data = await categoriesApiService.getAll();
      setCategories(data);
    } catch (err) {
      console.error('Error fetching categories:', err);
    }
  }, []);

  // Sử dụng API phân trang để lấy sản phẩm
  const fetchProducts = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Cập nhật searchTerm vào queryParams
      const params = { ...queryParams, searchTerm };
      
      // Sử dụng API phân trang
      const result: PagedResult<Product> = await productsApiService.getPagedProducts(params);
      
      setProducts(result.items);
      setTotalItems(result.totalCount);
      setTotalPages(result.totalPages);
      setCurrentPage(result.pageNumber);
      setPageSize(result.pageSize);
    } catch (err) {
      setError('Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.');
      console.error('Error fetching products:', err);
    } finally {
      setIsLoading(false);
    }
  }, [queryParams, searchTerm]);

  useEffect(() => {
    fetchProducts();
    fetchCategories();
  }, [fetchProducts, fetchCategories]);

  const handleDeleteProduct = async (id: string) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
      try {
        await productsApiService.delete(id);
        fetchProducts();
      } catch (err) {
        setError('Không thể xóa sản phẩm. Vui lòng thử lại sau.');
        console.error('Error deleting product:', err);
      }
    }
  };

  const handlePageChange = (page: number) => {
    setQueryParams(prev => ({ ...prev, pageNumber: page }));
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setQueryParams(prev => ({ ...prev, pageNumber: 1 }));
  };

  const handleFilterChange = (name: string, value: any) => {
    setQueryParams(prev => ({ ...prev, [name]: value, pageNumber: 1 }));
  };

  const handleSortChange = (field: string) => {
    setQueryParams(prev => ({
      ...prev,
      sortBy: field,
      sortDesc: prev.sortBy === field ? !prev.sortDesc : false,
      pageNumber: 1
    }));
  };

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Quản lý sản phẩm</h1>
        <Link
          to="/products/create"
          className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-md inline-flex items-center transition-colors duration-300"
        >
          <FiPlus className="mr-2" /> Thêm sản phẩm
        </Link>
      </div>

      <div className="mb-6">
        <div className="flex items-center justify-between">
          <form onSubmit={handleSearch} className="relative flex-1 max-w-md">
            <input
              type="text"
              placeholder="Tìm kiếm sản phẩm..."
              className="w-full py-2 pl-10 pr-4 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <FiSearch className="absolute left-3 top-3 text-gray-400" />
            <button type="submit" className="hidden">Tìm kiếm</button>
          </form>
          
          <button
            className="ml-3 flex items-center text-sm text-gray-600 hover:text-blue-600"
            onClick={() => setShowFilters(!showFilters)}
          >
            <FiFilter className="mr-1" /> Bộ lọc
          </button>
        </div>

        {showFilters && (
          <div className="mt-4 p-4 bg-gray-50 rounded-md border border-gray-200">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Danh mục</label>
                <select
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  onChange={(e) => handleFilterChange('categoryId', e.target.value || undefined)}
                >
                  <option value="">Tất cả danh mục</option>
                  {categories.map(category => (
                    <option key={category.id} value={category.id}>
                      {category.categoryName}
                    </option>
                  ))}
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Trạng thái</label>
                <select
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  onChange={(e) => handleFilterChange('status', e.target.value || undefined)}
                >
                  <option value="">Tất cả trạng thái</option>
                  <option value="Active">Đang hoạt động</option>
                  <option value="Inactive">Ngừng hoạt động</option>
                  <option value="Draft">Nháp</option>
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Sản phẩm nổi bật</label>
                <select
                  className="w-full border border-gray-300 rounded-md py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  onChange={(e) => {
                    const value = e.target.value;
                    handleFilterChange('isFeatured', value ? value === 'true' : undefined);
                  }}
                >
                  <option value="">Tất cả</option>
                  <option value="true">Có</option>
                  <option value="false">Không</option>
                </select>
              </div>
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className="mb-4 bg-red-50 text-red-600 p-3 rounded-md">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSortChange('productName')}>
                    Sản phẩm {queryParams.sortBy === 'productName' && (queryParams.sortDesc ? '▼' : '▲')}
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSortChange('productPrice')}>
                    Giá {queryParams.sortBy === 'productPrice' && (queryParams.sortDesc ? '▼' : '▲')}
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSortChange('productStock')}>
                    Tồn kho {queryParams.sortBy === 'productStock' && (queryParams.sortDesc ? '▼' : '▲')}
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Danh mục
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Trạng thái
                  </th>
                  <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Thao tác
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {products.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-4 text-center text-gray-500">
                      Không có sản phẩm nào
                    </td>
                  </tr>
                ) : (
                  products.map((product) => (
                    <tr key={product.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10">
                            {product.imageUrls && product.imageUrls.length > 0 ? (
                              <img className="h-10 w-10 rounded-md object-cover" src={product.imageUrls[0]} alt={product.productName} />
                            ) : (
                              <div className="h-10 w-10 bg-gray-200 rounded-md flex items-center justify-center text-gray-400">
                                No img
                              </div>
                            )}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">{product.productName}</div>
                            <div className="text-sm text-gray-500">SKU: {product.productSku}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">{product.productPrice.toLocaleString('vi-VN')} ₫</div>
                        {product.productDiscountPrice && (
                          <div className="text-xs text-gray-500 line-through">{product.productDiscountPrice.toLocaleString('vi-VN')} ₫</div>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">{product.productStock}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">{product.categoryName}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full
                          ${product.productStatus === 'Active' ? 'bg-green-100 text-green-800' : 
                            product.productStatus === 'Inactive' ? 'bg-red-100 text-red-800' : 'bg-gray-100 text-gray-800'}`}
                        >
                          {product.productStatus === 'Active' ? 'Đang hoạt động' : 
                           product.productStatus === 'Inactive' ? 'Ngừng hoạt động' : 'Nháp'}
                        </span>
                        {product.isFeatured && (
                          <span className="ml-2 px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-yellow-100 text-yellow-800">
                            Nổi bật
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <Link
                          to={`/products/edit/${product.id}`}
                          className="text-blue-600 hover:text-blue-900 mr-3"
                        >
                          <FiEdit className="inline" />
                        </Link>
                        <button
                          onClick={() => handleDeleteProduct(product.id)}
                          className="text-red-600 hover:text-red-900"
                        >
                          <FiTrash2 className="inline" />
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {totalPages > 0 && (
            <div className="mt-6 flex items-center justify-between">
              <div className="text-sm text-gray-700">
                Hiển thị <span className="font-medium">{(currentPage - 1) * pageSize + 1}</span> đến{' '}
                <span className="font-medium">{Math.min(currentPage * pageSize, totalItems)}</span> trong{' '}
                <span className="font-medium">{totalItems}</span> sản phẩm
              </div>
              
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                  className={`px-3 py-1 rounded-md ${
                    currentPage === 1 
                      ? 'bg-gray-100 text-gray-400 cursor-not-allowed' 
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                  }`}
                >
                  Trước
                </button>
                
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  // Hiển thị 5 trang xung quanh trang hiện tại
                  let pageToShow = currentPage;
                  if (currentPage < 3) {
                    pageToShow = i + 1;
                  } else if (currentPage > totalPages - 2) {
                    pageToShow = totalPages - 4 + i;
                  } else {
                    pageToShow = currentPage - 2 + i;
                  }
                  
                  // Đảm bảo trang hiển thị nằm trong phạm vi hợp lệ
                  if (pageToShow > 0 && pageToShow <= totalPages) {
                    return (
                      <button
                        key={pageToShow}
                        onClick={() => handlePageChange(pageToShow)}
                        className={`px-3 py-1 rounded-md ${
                          currentPage === pageToShow
                            ? 'bg-blue-600 text-white'
                            : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                        }`}
                      >
                        {pageToShow}
                      </button>
                    );
                  }
                  return null;
                })}
                
                <button
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className={`px-3 py-1 rounded-md ${
                    currentPage === totalPages
                      ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                  }`}
                >
                  Tiếp
                </button>
              </div>
              
              <div className="text-sm">
                <select
                  value={pageSize}
                  onChange={(e) => {
                    const newPageSize = Number(e.target.value);
                    setPageSize(newPageSize);
                    setQueryParams(prev => ({ ...prev, pageSize: newPageSize, pageNumber: 1 }));
                  }}
                  className="border rounded-md px-2 py-1 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="5">5 / trang</option>
                  <option value="10">10 / trang</option>
                  <option value="20">20 / trang</option>
                  <option value="50">50 / trang</option>
                </select>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default ProductList; 