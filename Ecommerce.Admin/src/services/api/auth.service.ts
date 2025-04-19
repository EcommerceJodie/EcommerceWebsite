import apiClient from '../http/api-client';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  token: string;
  refreshToken: string;
  roles: string[];
  createdAt: string;
}

class AuthApiService {
  private readonly baseEndpoint = '/api/auth';
  
  
  public async login(credentials: LoginRequest): Promise<UserResponse> {
    return apiClient.post<UserResponse>(`${this.baseEndpoint}/login`, credentials);
  }
  
  
  public async getCurrentUser(): Promise<UserResponse> {
    return apiClient.get<UserResponse>(`${this.baseEndpoint}/me`);
  }
  
  
  public async logout(): Promise<{message: string}> {
    return apiClient.post<{message: string}>(`${this.baseEndpoint}/logout`);
  }

  public async refreshToken(refreshToken: string): Promise<UserResponse> {
    return apiClient.post<UserResponse>(`${this.baseEndpoint}/refresh-token`, { refreshToken });
  }
}

export const authApiService = new AuthApiService(); 