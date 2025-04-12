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
    
    formData.append('CategoryImageUrl', category.categoryImageUrl || '');
    
    formData.append('DisplayOrder', category.displayOrder.toString());
    formData.append('IsActive', category.isActive.toString());
    
    if (category.categoryImage) formData.append('CategoryImage', category.categoryImage);
    
    return apiClient.postForm<Category>(this.baseEndpoint, formData);
  }
  

  public async update(id: string, category: UpdateCategoryDto): Promise<Category> {
    const formData = new FormData();
    
    formData.append('Id', id);
    formData.append('CategoryName', category.categoryName);
    if (category.categoryDescription) formData.append('CategoryDescription', category.categoryDescription);
    if (category.categorySlug) formData.append('CategorySlug', category.categorySlug);
    
    formData.append('CategoryImageUrl', category.categoryImageUrl || '');
    
    formData.append('DisplayOrder', category.displayOrder.toString());
    formData.append('IsActive', category.isActive.toString());
    
    if (category.categoryImage) formData.append('CategoryImage', category.categoryImage);
    
    return apiClient.putForm<Category>(`${this.baseEndpoint}/${id}`, formData);
  }
  

  public async delete(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/${id}`);
  }
}

export const categoriesApiService = new CategoriesApiService(); 