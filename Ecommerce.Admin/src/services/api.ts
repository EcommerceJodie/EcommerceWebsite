import { Category } from '../models/Category';
import { Product } from '../models/Product';
import { Customer } from '../models/Customer';

const API_URL = 'http://localhost:5000/api'; 

// Chức năng fetch chung vớ xử lý lỗi
async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_URL}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
    },
    ...options,
  });

  if (!response.ok) {
    throw new Error(`API Error: ${response.status}`);
  }

  return response.json();
}

export const productsApi = {
  getAll: () => fetchApi<Product[]>('/products'),
  getById: (id: number) => fetchApi<Product>(`/products/${id}`),
  create: (product: Omit<Product, 'id' | 'createdAt' | 'updatedAt'>) =>
    fetchApi<Product>('/products', {
      method: 'POST',
      body: JSON.stringify(product),
    }),
  update: (id: number, product: Partial<Product>) =>
    fetchApi<Product>(`/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(product),
    }),
  delete: (id: number) =>
    fetchApi<void>(`/products/${id}`, {
      method: 'DELETE',
    }),
};

// API Categories
export const categoriesApi = {
  getAll: () => fetchApi<Category[]>('/categories'),
  getById: (id: number) => fetchApi<Category>(`/categories/${id}`),
  create: (category: Omit<Category, 'id' | 'createdAt' | 'updatedAt'>) =>
    fetchApi<Category>('/categories', {
      method: 'POST',
      body: JSON.stringify(category),
    }),
  update: (id: number, category: Partial<Category>) =>
    fetchApi<Category>(`/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(category),
    }),
  delete: (id: number) =>
    fetchApi<void>(`/categories/${id}`, {
      method: 'DELETE',
    }),
};

// API Customers
export const customersApi = {
  getAll: () => fetchApi<Customer[]>('/customers'),
  getById: (id: string) => fetchApi<Customer>(`/customers/${id}`),
}; 