export interface Category {
  id: string;
  categoryName: string;
  categoryDescription?: string;
  categorySlug?: string;
  categoryImageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCategoryDto {
  categoryName: string;
  categoryDescription?: string;
  categorySlug?: string;
  categoryImageUrl?: string;
  categoryImage?: File;
  displayOrder: number;
  isActive: boolean;
}

export interface UpdateCategoryDto extends CreateCategoryDto {
  id: string;
} 