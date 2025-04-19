export interface Product {
  id: string;
  productName: string;
  productDescription: string;
  productSlug?: string;
  productPrice: number;
  productDiscountPrice?: number;
  productStock: number;
  productSku: string;
  productStatus: string;
  isFeatured: boolean;
  metaTitle?: string;
  metaDescription?: string;
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt?: string;
  imageUrls: string[];
}

export interface ProductImage {
  id: string;
  productId: string;
  imageUrl: string;
  imageAltText?: string;
  isMainImage: boolean;
  displayOrder: number;
} 