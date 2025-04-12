import apiClient from '../http/api-client';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  token: string;
  roles: string[];
  createdAt: string;
}

class AuthApiService {
  private readonly baseEndpoint = '/auth';
  
  
  public async login(credentials: LoginRequest): Promise<UserResponse> {
    return apiClient.post<UserResponse>(`${this.baseEndpoint}/login`, credentials);
  }
  
  
  public async getCurrentUser(): Promise<UserResponse> {
    return apiClient.get<UserResponse>(`${this.baseEndpoint}/me`);
  }
  
  
  public async logout(): Promise<{message: string}> {
    return apiClient.post<{message: string}>(`${this.baseEndpoint}/logout`);
  }
}

export const authApiService = new AuthApiService(); 