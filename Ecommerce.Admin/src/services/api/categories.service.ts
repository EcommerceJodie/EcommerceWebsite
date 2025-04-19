import apiClient from '../http/api-client';
import { Category, CreateCategoryDto, UpdateCategoryDto } from '../../models/Category';

class CategoriesApiService {
  private readonly baseEndpoint = '/api/categories';
  
 
  public async getAll(): Promise<Category[]> {
    return apiClient.get<Category[]>(this.baseEndpoint);
  }
  
 
  public async getActive(): Promise<Category[]> {
    return apiClient.get<Category[]>(`${this.baseEndpoint}/active`);
  }
  

  public async getById(id: string): Promise<Category> {
    return apiClient.get<Category>(`${this.baseEndpoint}/${id}`);
  }
  
 
  public async create(category: CreateCategoryDto): Promise<Category> {
    const formData = new FormData();
    
    formData.append('CategoryName', category.categoryName);
    if (category.categoryDescription) formData.append('CategoryDescription', category.categoryDescription);
    if (category.categorySlug) formData.append('CategorySlug', category.categorySlug);
    
    formData.append('DisplayOrder', category.displayOrder.toString());
    formData.append('IsActive', category.isActive.toString());
    
    if (category.categoryImage) {
      formData.append('CategoryImage', category.categoryImage);
    } else if (category.categoryImageUrl) {
      formData.append('CategoryImageUrl', category.categoryImageUrl);
    }
    
    return apiClient.postForm<Category>(this.baseEndpoint, formData);
  }
  

  public async update(id: string, category: UpdateCategoryDto): Promise<Category> {
    const formData = new FormData();
    
    formData.append('Id', id);
    formData.append('CategoryName', category.categoryName);
    if (category.categoryDescription) formData.append('CategoryDescription', category.categoryDescription);
    if (category.categorySlug) formData.append('CategorySlug', category.categorySlug);
    
    formData.append('DisplayOrder', category.displayOrder.toString());
    formData.append('IsActive', category.isActive.toString());
    
    const hasNewImage = !!category.categoryImage;
    
    if (hasNewImage && category.categoryImage) {
      formData.append('CategoryImage', category.categoryImage);
    } else {
      const emptyBlob = new Blob([], { type: 'application/octet-stream' });
      const emptyFile = new File([emptyBlob], 'empty.txt', { type: 'application/octet-stream' });
      formData.append('CategoryImage', emptyFile);
      
      if (category.categoryImageUrl) {
        let imageUrl = category.categoryImageUrl;
        if (imageUrl.includes('?')) {
          imageUrl = imageUrl.substring(0, imageUrl.indexOf('?'));
          console.log(`Đã cắt URL: ${imageUrl}`);
        }
        formData.append('CategoryImageUrl', imageUrl);
      }
    }
    
    console.log('FormData trước khi gửi đi:');
    for (const pair of formData.entries()) {
      if (pair[1] instanceof File) {
        const file = pair[1] as File;
        console.log(`${pair[0]}: File - ${file.name} (${file.size} bytes)`);
      } else {
        console.log(`${pair[0]}: ${pair[1]}`);
      }
    }
    
    return apiClient.putForm<Category>(`${this.baseEndpoint}/${id}?keepExistingImage=${!hasNewImage}`, formData);
  }
  

  public async delete(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/${id}`);
  }

  public async getImagePresignedUrl(id: string, expiryMinutes: number = 30): Promise<string> {
    const response = await apiClient.get<{ url: string }>(`${this.baseEndpoint}/${id}/image-url?expiryMinutes=${expiryMinutes}`);
    return response.url;
  }
}

export const categoriesApiService = new CategoriesApiService(); 