import apiClient from '../http/api-client';
import { Customer } from '../../models/Customer';

class CustomersApiService {
  private readonly baseEndpoint = '/api/customers';
  

  public async getAll(): Promise<Customer[]> {
    return apiClient.get<Customer[]>(this.baseEndpoint);
  }
  

  public async getById(id: string): Promise<Customer> {
    return apiClient.get<Customer>(`${this.baseEndpoint}/${id}`);
  }
}

export const customersApiService = new CustomersApiService(); 