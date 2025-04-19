export interface MenuConfig {
  id: string;
  categoryId: string;
  categoryName: string;
  categorySlug?: string;
  categoryImageUrl?: string;
  isVisible: boolean;
  displayOrder: number;
  customName?: string;
  icon?: string;
  isMainMenu: boolean;
  parentId?: string | null;
  children: MenuConfig[];
  createdAt: string;
  updatedAt?: string;
}

export interface CreateMenuConfigDto {
  categoryId: string;
  isVisible: boolean;
  displayOrder: number;
  customName?: string;
  icon?: string;
  isMainMenu: boolean;
  parentId?: string | null;
}

export interface UpdateMenuConfigDto extends CreateMenuConfigDto {
  id: string;
} 