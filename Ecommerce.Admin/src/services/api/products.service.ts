import apiClient from '../http/api-client';
import { Product } from '../../models/Product';

class ProductsApiService {
  private readonly baseEndpoint = '/products';
  

  public async getAll(): Promise<Product[]> {
    return apiClient.get<Product[]>(this.baseEndpoint);
  }
  

  public async getById(id: number): Promise<Product> {
    return apiClient.get<Product>(`${this.baseEndpoint}/${id}`);
  }
  

  public async create(product: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>): Promise<Product> {
    return apiClient.post<Product>(this.baseEndpoint, product);
  }
  

  public async update(id: number, product: Partial<Product>): Promise<Product> {
    return apiClient.put<Product>(`${this.baseEndpoint}/${id}`, product);
  }
  

  public async delete(id: number): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/${id}`);
  }
}

export const productsApiService = new ProductsApiService(); 