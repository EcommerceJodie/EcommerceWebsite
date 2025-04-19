import apiClient from '../http/api-client';
import { MenuConfig, CreateMenuConfigDto, UpdateMenuConfigDto } from '../../models/MenuConfig';

export interface MenuConfigQueryParams {
  isMainMenu?: boolean;
  parentId?: string | null;
}

class MenuConfigsApiService {
  private readonly baseEndpoint = '/api/MenuConfigs';

  public async getAll(): Promise<MenuConfig[]> {
    return apiClient.get<MenuConfig[]>(this.baseEndpoint);
  }

  public async getMenuTree(): Promise<MenuConfig[]> {
    return apiClient.get<MenuConfig[]>(`${this.baseEndpoint}/tree`);
  }

  public async getRootMenuConfigs(isMainMenu: boolean = true): Promise<MenuConfig[]> {
    return apiClient.get<MenuConfig[]>(`${this.baseEndpoint}/root?isMainMenu=${isMainMenu}`);
  }

  public async getVisibleMenuConfigs(isMainMenu: boolean = true): Promise<MenuConfig[]> {
    return apiClient.get<MenuConfig[]>(`${this.baseEndpoint}/visible?isMainMenu=${isMainMenu}`);
  }

  public async getByParentId(parentId: string | null): Promise<MenuConfig[]> {
    const endpoint = parentId 
      ? `${this.baseEndpoint}/by-parent/${parentId}` 
      : `${this.baseEndpoint}/by-parent/null`;
    return apiClient.get<MenuConfig[]>(endpoint);
  }

  public async getById(id: string): Promise<MenuConfig> {
    return apiClient.get<MenuConfig>(`${this.baseEndpoint}/${id}`);
  }

  public async getByCategoryId(categoryId: string, isMainMenu: boolean = true): Promise<MenuConfig> {
    return apiClient.get<MenuConfig>(`${this.baseEndpoint}/by-category/${categoryId}?isMainMenu=${isMainMenu}`);
  }

  public async create(menuConfig: CreateMenuConfigDto): Promise<MenuConfig> {
    const dataToSend = { 
      ...menuConfig,
      icon: menuConfig.icon || "",
      parentId: menuConfig.parentId === '' ? null : menuConfig.parentId,
      displayOrder: typeof menuConfig.displayOrder === 'string' 
        ? parseInt(menuConfig.displayOrder as string, 10) 
        : menuConfig.displayOrder
    };
    
    console.log('Request data (sanitized):', JSON.stringify(dataToSend));
    
    return apiClient.post<MenuConfig>(this.baseEndpoint, dataToSend);
  }

  public async update(id: string, menuConfig: UpdateMenuConfigDto): Promise<MenuConfig> {
    const dataToSend = { 
      ...menuConfig,
      icon: menuConfig.icon || "",
      parentId: menuConfig.parentId === '' ? null : menuConfig.parentId,
      displayOrder: typeof menuConfig.displayOrder === 'string' 
        ? parseInt(menuConfig.displayOrder as string, 10) 
        : menuConfig.displayOrder
    };
    
    console.log('Update request data (sanitized):', JSON.stringify(dataToSend));
    
    return apiClient.put<MenuConfig>(`${this.baseEndpoint}/${id}`, dataToSend);
  }

  public async delete(id: string): Promise<void> {
    return apiClient.delete<void>(`${this.baseEndpoint}/${id}`);
  }
}

export const menuConfigsApiService = new MenuConfigsApiService(); 