import axios, { AxiosError, AxiosRequestConfig, AxiosResponse } from 'axios';

const API_URL = import.meta.env.VITE_API_URL;

console.log('API URL Configuration:', API_URL);

axios.defaults.baseURL = API_URL;
axios.defaults.headers.common['Content-Type'] = 'application/json';
axios.defaults.timeout = 30000; 

// Biến kiểm soát việc đang refresh token hay không
let isRefreshing = false;
// Mảng các request đang chờ token mới
let failedQueue: {
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
  config: AxiosRequestConfig;
}[] = [];

// Xử lý hàng đợi request khi refresh token thành công
const processQueue = (error: AxiosError | null, token: string | null = null) => {
  failedQueue.forEach(request => {
    if (error) {
      request.reject(error);
    } else if (token) {
      // Cập nhật token trong request đang chờ
      if (request.config.headers) {
        request.config.headers['Authorization'] = `Bearer ${token}`;
      }
      request.resolve(axios(request.config));
    }
  });
  
  failedQueue = [];
};

// Helper function để lấy token từ localStorage
const getToken = (): string | null => {
  return localStorage.getItem('token');
};

const logFormData = (formData: FormData) => {
  console.log('FormData contents:');
  for (const pair of formData.entries()) {
    if (pair[1] instanceof File) {
      const file = pair[1] as File;
      console.log(`${pair[0]}: File - ${file.name} (${file.size} bytes)`);
    } else {
      console.log(`${pair[0]}: ${pair[1]}`);
    }
  }
};

axios.interceptors.request.use(
  (config) => {
    const fullUrl = config.baseURL && config.url ? `${config.baseURL}${config.url}` : config.url || 'unknown URL';
    console.log('Request URL:', fullUrl);
    console.log('Request method:', config.method?.toUpperCase() || 'unknown method');
    console.log('Request headers:', config.headers);
    
    if (config.data) {
      if (config.data instanceof FormData) {
        logFormData(config.data);
      } else {
        console.log('Request data:', config.data);
      }
    }
    
    const token = getToken();
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    console.error('Request interceptor error:', error);
    return Promise.reject(error);
  }
);

axios.interceptors.response.use(
  (response) => {
    console.log('Response status:', response.status);
    console.log('Response data:', response.data);
    return response;
  },
  async (error) => {
    console.error('Response error:', error);
    console.error('Error config URL:', error.config?.url);
    console.error('Error config method:', error.config?.method?.toUpperCase());
    
    const originalRequest = error.config;
    
    if (error.response) {
      console.error('Error response status:', error.response.status);
      console.error('Error response data:', error.response.data);
      console.error('Error response headers:', error.response.headers);
      
      // Xử lý khi token hết hạn (401)
      if (error.response.status === 401 && 
          !originalRequest._retry && 
          originalRequest.url !== '/api/auth/refresh-token' && 
          originalRequest.url !== '/api/auth/login') {
        
        if (isRefreshing) {
          // Nếu đang refresh, thêm request vào hàng đợi
          return new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject, config: originalRequest });
          });
        }
        
        originalRequest._retry = true;
        isRefreshing = true;
        
        try {
          // Lấy refresh token từ localStorage
          const refreshToken = localStorage.getItem('refreshToken');
          if (!refreshToken) {
            // Không có refresh token, chuyển người dùng về trang đăng nhập
            window.location.href = '/login';
            return Promise.reject(error);
          }
          
          // Gọi API refresh token
          const response = await axios.post('/api/auth/refresh-token', { refreshToken });
          const { token, refreshToken: newRefreshToken } = response.data;
          
          // Lưu token mới vào localStorage
          localStorage.setItem('token', token);
          localStorage.setItem('refreshToken', newRefreshToken);
          
          // Cập nhật token cho request hiện tại
          axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
          originalRequest.headers['Authorization'] = `Bearer ${token}`;
          
          // Xử lý các request trong hàng đợi
          processQueue(null, token);
          
          // Thực hiện lại request ban đầu
          return axios(originalRequest);
        } catch (refreshError) {
          // Xử lý lỗi refresh token
          processQueue(refreshError as AxiosError, null);
          
          // Xóa token và chuyển về trang đăng nhập
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('adminUser');
          
          window.location.href = '/login';
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }
    } else if (error.request) {
      console.error('Error request:', error.request);
    } else {
      console.error('Error message:', error.message);
    }
    
    const customError = {
      status: error.response?.status,
      data: error.response?.data,
      message: error.response?.data?.message || error.message || 'Có lỗi xảy ra',
    };
    return Promise.reject(customError);
  }
);

class ApiClient {

  async get<T>(endpoint: string, params?: Record<string, string>): Promise<T> {
    try {
      const response = await axios.get<T>(endpoint, { params });
      return response.data;
    } catch (error) {
      console.error(`GET ${endpoint} failed:`, error);
      throw error;
    }
  }


  async post<T>(endpoint: string, data?: unknown): Promise<T> {
    try {
      const response = await axios.post<T>(endpoint, data);
      return response.data;
    } catch (error) {
      console.error(`POST ${endpoint} failed:`, error);
      throw error;
    }
  }

  async postForm<T>(endpoint: string, formData: FormData): Promise<T> {
    try {
      console.log(`Sending FormData to ${endpoint}:`);
      logFormData(formData);
      
      const response = await axios.post<T>(endpoint, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    } catch (error) {
      console.error(`POST FORM ${endpoint} failed:`, error);
      throw error;
    }
  }


  async put<T>(endpoint: string, data?: unknown): Promise<T> {
    try {
      const response = await axios.put<T>(endpoint, data);
      return response.data;
    } catch (error) {
      console.error(`PUT ${endpoint} failed:`, error);
      throw error;
    }
  }
  

  async putForm<T>(endpoint: string, formData: FormData): Promise<T> {
    try {
      console.log(`Sending FormData to ${endpoint} (PUT):`);
      logFormData(formData);
      
      const response = await axios.put<T>(endpoint, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    } catch (error) {
      console.error(`PUT FORM ${endpoint} failed:`, error);
      throw error;
    }
  }


  async delete<T>(endpoint: string): Promise<T> {
    try {
      const response = await axios.delete<T>(endpoint);
      return response.data;
    } catch (error) {
      console.error(`DELETE ${endpoint} failed:`, error);
      throw error;
    }
  }


  async patch<T>(endpoint: string, data?: unknown): Promise<T> {
    try {
      const response = await axios.patch<T>(endpoint, data);
      return response.data;
    } catch (error) {
      console.error(`PATCH ${endpoint} failed:`, error);
      throw error;
    }
  }


  async downloadFile(endpoint: string): Promise<Blob> {
    try {
      const response = await axios.get(endpoint, {
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      console.error(`Download file from ${endpoint} failed:`, error);
      throw error;
    }
  }
}

const apiClient = new ApiClient();
export default apiClient; 