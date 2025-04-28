import apiClient from '../http/api-client';
import { Product, ProductImage } from '../../models/Product';

export interface ProductQueryParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDesc?: boolean;
  searchTerm?: string;
  categoryId?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  inStock?: boolean;
  status?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

class ProductsApiService {
  private readonly baseEndpoint = '/api/products';
  
  /**
   * Lấy danh sách sản phẩm có phân trang
   * @param params Tham số truy vấn
   * @returns 
   */
  public async getPagedProducts(params: ProductQueryParams = {}): Promise<PagedResult<Product>> {
    const queryParams = new URLSearchParams();
    
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortDesc !== undefined) queryParams.append('sortDesc', params.sortDesc ? 'true' : 'false');
    if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    if (params.categoryId) queryParams.append('categoryId', params.categoryId);
    if (params.minPrice !== undefined) queryParams.append('minPrice', params.minPrice.toString());
    if (params.maxPrice !== undefined) queryParams.append('maxPrice', params.maxPrice.toString());
    if (params.isFeatured !== undefined) queryParams.append('isFeatured', params.isFeatured ? 'true' : 'false');
    if (params.inStock !== undefined) queryParams.append('inStock', params.inStock ? 'true' : 'false');
    if (params.status) queryParams.append('status', params.status);
    
    const queryString = queryParams.toString();
    const url = queryString ? `${this.baseEndpoint}?${queryString}` : this.baseEndpoint;
    
    return apiClient.get<PagedResult<Product>>(url);
  }
  
  /**
   * Lấy tất cả sản phẩm (không phân trang)
   * @returns 
   */
  public async getAll(): Promise<Product[]> {
    return apiClient.get<Product[]>(`${this.baseEndpoint}/all`);
  }
  
  /**
   * Lấy sản phẩm theo danh mục có phân trang
   * @param categoryId ID của danh mục
   * @param params Tham số phân trang
   * @returns 
   */
  public async getProductsByCategory(
    categoryId: string,
    params: Omit<ProductQueryParams, 'categoryId'> = {}
  ): Promise<PagedResult<Product>> {
    const queryParams = new URLSearchParams();
    
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortDesc !== undefined) queryParams.append('sortDesc', params.sortDesc ? 'true' : 'false');
    if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    
    const queryString = queryParams.toString();
    const url = queryString 
      ? `${this.baseEndpoint}/Category/${categoryId}?${queryString}` 
      : `${this.baseEndpoint}/Category/${categoryId}`;
    
    return apiClient.get<PagedResult<Product>>(url);
  }
  
  /**
   * Lấy tất cả sản phẩm theo danh mục (không phân trang)
   * @param categoryId ID của danh mục
   * @returns 
   */
  public async getAllProductsByCategory(categoryId: string): Promise<Product[]> {
    return apiClient.get<Product[]>(`${this.baseEndpoint}/Category/${categoryId}/all`);
  }
  
  /**
   * Lấy thông tin chi tiết của sản phẩm
   * @param id ID của sản phẩm
   * @returns 
   */
  public async getById(id: string): Promise<Product> {
    return apiClient.get<Product>(`${this.baseEndpoint}/${id}`);
  }
  
  /**
   * Tạo sản phẩm mới
   * @param formData Form data chứa thông tin sản phẩm và hình ảnh
   * @returns 
   */
  public async create(formData: FormData): Promise<Product> {
    return apiClient.postForm<Product>(this.baseEndpoint, formData);
  }
  
  /**
   * Cập nhật sản phẩm
   * @param id ID của sản phẩm
   * @param formData Form data chứa thông tin cập nhật và hình ảnh
   * @returns 
   */
  public async update(id: string, formData: FormData): Promise<Product> {
    return apiClient.putForm<Product>(`${this.baseEndpoint}/${id}`, formData);
  }
  
  /**
   * Xóa sản phẩm
   * @param id ID của sản phẩm
   * @returns 
   */
  public async delete(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/${id}`);
  }
  
  /**
   * Xóa nhiều sản phẩm cùng lúc
   * @param productIds Danh sách ID sản phẩm cần xóa
   * @returns 
   */
  public async deleteMultiple(productIds: string[]): Promise<void> {
    return apiClient.post<void>(`${this.baseEndpoint}/batch`, { productIds });
  }
  
  /**
   * Nhân bản sản phẩm
   * @param productId ID của sản phẩm cần nhân bản
   * @param options Tùy chọn nhân bản
   * @returns Sản phẩm mới được tạo ra
   */
  public async duplicateProduct(
    productId: string, 
    options: { 
      newProductName?: string;
      newProductSku?: string;
      newProductSlug?: string;
      copyImages?: boolean;
    } = {}
  ): Promise<Product> {
    return apiClient.post<Product>(`${this.baseEndpoint}/${productId}/duplicate`, options);
  }
  
  /**
   * Thêm hình ảnh cho sản phẩm
   * @param productId ID của sản phẩm
   * @param formData Form data chứa hình ảnh
   * @returns 
   */
  public async addProductImage(productId: string, formData: FormData): Promise<ProductImage> {
    return apiClient.postForm<ProductImage>(`${this.baseEndpoint}/${productId}/Images`, formData);
  }
  
  /**
   * Xóa hình ảnh sản phẩm
   * @param imageId ID của hình ảnh
   * @returns 
   */
  public async deleteProductImage(imageId: string): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/Images/${imageId}`);
  }
  
  /**
   * Đặt hình ảnh làm hình chính
   * @param imageId ID của hình ảnh
   * @returns 
   */
  public async setMainImage(imageId: string): Promise<void> {
    return apiClient.put<void>(`${this.baseEndpoint}/Images/${imageId}/SetMain`, {});
  }

  /**
   * Lấy presigned URL cho hình ảnh sản phẩm
   * @param imageId ID của hình ảnh
   * @param expiryMinutes Thời gian hết hạn của URL (phút)
   * @returns 
   */
  public async getImagePresignedUrl(imageId: string, expiryMinutes: number = 30): Promise<string> {
    const response = await apiClient.get<{ url: string }>(`${this.baseEndpoint}/Images/${imageId}/presigned-url?expiryMinutes=${expiryMinutes}`);
    return response.url;
  }

  /**
   * Lấy presigned URL cho hình ảnh chính của sản phẩm
   * @param productId ID của sản phẩm
   * @param expiryMinutes Thời gian hết hạn của URL (phút)
   * @returns 
   */
  public async getMainImagePresignedUrl(productId: string, expiryMinutes: number = 30): Promise<string> {
    const response = await apiClient.get<{ url: string }>(`${this.baseEndpoint}/${productId}/main-image-url?expiryMinutes=${expiryMinutes}`);
    return response.url;
  }
  
  /**
   * Lấy danh sách sản phẩm nổi bật
   * @returns 
   */
  public async getFeaturedProducts(): Promise<Product[]> {
    return apiClient.get<Product[]>(`${this.baseEndpoint}/Featured`);
  }
}

export const productsApiService = new ProductsApiService(); 