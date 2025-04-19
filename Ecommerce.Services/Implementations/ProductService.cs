using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Core.Validators.ProductValidators;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMinioService _minioService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductService(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMinioService minioService,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _minioService = minioService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(ProductQueryDto queryDto)
        {

            var query = _productRepository.Ts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsMainImage == true))
                .Where(p => !p.IsDeleted);


            if (queryDto.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == queryDto.CategoryId);
            }

            if (queryDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice >= queryDto.MinPrice);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice <= queryDto.MaxPrice);
            }

            if (queryDto.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == queryDto.IsFeatured);
            }

            if (queryDto.InStock.HasValue)
            {
                query = query.Where(p => (p.ProductStock > 0) == queryDto.InStock);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Status))
            {
                if (Enum.TryParse<ProductStatus>(queryDto.Status, out var status))
                {
                    query = query.Where(p => p.ProductStatus == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var searchTerm = queryDto.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    p.ProductDescription.ToLower().Contains(searchTerm) ||
                    p.ProductSku.ToLower().Contains(searchTerm) ||
                    p.Category.CategoryName.ToLower().Contains(searchTerm)
                );
            }


            var totalCount = await query.CountAsync();


            query = queryDto.SortBy.ToLower() switch
            {
                "name" => queryDto.SortDesc ? 
                    query.OrderByDescending(p => p.ProductName) : 
                    query.OrderBy(p => p.ProductName),
                "price" => queryDto.SortDesc ? 
                    query.OrderByDescending(p => p.ProductPrice) : 
                    query.OrderBy(p => p.ProductPrice),
                "stock" => queryDto.SortDesc ? 
                    query.OrderByDescending(p => p.ProductStock) : 
                    query.OrderBy(p => p.ProductStock),
                "category" => queryDto.SortDesc ? 
                    query.OrderByDescending(p => p.Category.CategoryName) : 
                    query.OrderBy(p => p.Category.CategoryName),
                _ => queryDto.SortDesc ? 
                    query.OrderByDescending(p => p.CreatedAt) : 
                    query.OrderBy(p => p.CreatedAt)
            };


            var products = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync();


            var productDtos = _mapper.Map<List<ProductDto>>(products);
            

            await ApplyPresignedUrlToProductsAsync(productDtos);


            var result = new PagedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = queryDto.PageNumber,
                PageSize = queryDto.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)queryDto.PageSize)
            };

            return result;
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, PaginationRequestDto pagination)
        {

            var query = _productRepository.Ts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => !p.IsDeleted && p.CategoryId == categoryId);


            if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
            {
                var searchTerm = pagination.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    p.ProductDescription.ToLower().Contains(searchTerm) ||
                    p.ProductSku.ToLower().Contains(searchTerm)
                );
            }


            var totalCount = await query.CountAsync();


            query = pagination.SortBy.ToLower() switch
            {
                "name" => pagination.SortDesc ? 
                    query.OrderByDescending(p => p.ProductName) : 
                    query.OrderBy(p => p.ProductName),
                "price" => pagination.SortDesc ? 
                    query.OrderByDescending(p => p.ProductPrice) : 
                    query.OrderBy(p => p.ProductPrice),
                "stock" => pagination.SortDesc ? 
                    query.OrderByDescending(p => p.ProductStock) : 
                    query.OrderBy(p => p.ProductStock),
                _ => pagination.SortDesc ? 
                    query.OrderByDescending(p => p.CreatedAt) : 
                    query.OrderBy(p => p.CreatedAt)
            };


            var products = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();


            var productDtos = _mapper.Map<List<ProductDto>>(products);
            

            await ApplyPresignedUrlToProductsAsync(productDtos);


            var result = new PagedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
            };

            return result;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.Ts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            await ApplyPresignedUrlToProductsAsync(productDtos);

            return productDtos;
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductWithRelatedAsync(id);
            
            if (product == null || product.IsDeleted)
            {
                throw new EntityNotFoundException("Sản phẩm", id);
            }

            var productDto = _mapper.Map<ProductDto>(product);
            
            await ApplyPresignedUrlToProductAsync(productDto);

            return productDto;
        }

        public async Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            await ApplyPresignedUrlToProductsAsync(productDtos);
            
            return productDtos;
        }

        public async Task<List<ProductDto>> GetFeaturedProductsAsync()
        {
            var products = await _productRepository.GetFeaturedProductsAsync();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            await ApplyPresignedUrlToProductsAsync(productDtos);
            
            return productDtos;
        }

        private async Task ApplyPresignedUrlToProductsAsync(List<ProductDto> products)
        {
            try
            {
                foreach (var product in products)
                {
                    await ApplyPresignedUrlToProductAsync(product);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tạo presigned URL cho danh sách sản phẩm: {ex.Message}");
            }
        }
        
        private async Task ApplyPresignedUrlToProductAsync(ProductDto product)
        {
            try
            {
                if (product.ImageUrls != null && product.ImageUrls.Count > 0)
                {
                    var updatedUrls = new List<string>();
                    
                    foreach (var imageUrl in product.ImageUrls)
                    {
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var objectName = GetObjectNameFromUrl(imageUrl);
                            if (!string.IsNullOrEmpty(objectName))
                            {
                                var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, 60);
                                updatedUrls.Add(presignedUrl);
                            }
                            else
                            {
                                updatedUrls.Add(imageUrl);
                            }
                        }
                    }
                    
                    product.ImageUrls = updatedUrls;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tạo presigned URL cho sản phẩm: {ex.Message}");
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var validationResult = await _createValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Ecommerce.Core.Exceptions.ValidationException(errors);
            }

            var productImageRepository = _unitOfWork.Repository<ProductImage>();
            

            var product = _mapper.Map<Product>(productDto);
            product.ProductImages = new List<ProductImage>(); 
            
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _productRepository.Add(product);
                
                if (productDto.MainImage != null)
                {
                    var fileName = $"products/{product.Id}/main_{Guid.NewGuid()}{System.IO.Path.GetExtension(productDto.MainImage.FileName)}";
                    var imageUrl = await _minioService.UploadFileAsync(productDto.MainImage, fileName);
                    
                    var mainImage = new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        ImageAltText = product.ProductName,
                        IsMainImage = true,
                        DisplayOrder = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    productImageRepository.Add(mainImage);
                }
                
                if (productDto.ProductImages != null && productDto.ProductImages.Count > 0)
                {
                    int order = 1;
                    foreach (var imageFile in productDto.ProductImages)
                    {
                        var fileName = $"products/{product.Id}/{order}_{Guid.NewGuid()}{System.IO.Path.GetExtension(imageFile.FileName)}";
                        var imageUrl = await _minioService.UploadFileAsync(imageFile, fileName);
                        
                        var productImage = new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imageUrl,
                            ImageAltText = $"{product.ProductName} - {order}",
                            IsMainImage = false,
                            DisplayOrder = order,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        productImageRepository.Add(productImage);
                        order++;
                    }
                }
                
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                var createdProduct = await _productRepository.GetProductWithRelatedAsync(product.Id);
                return _mapper.Map<ProductDto>(createdProduct);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Ecommerce.Core.Exceptions.ValidationException(errors);
            }

            var product = await _productRepository.GetByIdAsync(productDto.Id);
            
            if (product == null || product.IsDeleted)
            {
                throw new EntityNotFoundException("Sản phẩm", productDto.Id);
            }

            _mapper.Map(productDto, product);
            
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _productRepository.Update(product);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var productImageRepository = _unitOfWork.Repository<ProductImage>();
            
            var product = await _productRepository.GetProductWithRelatedAsync(id);
            
            if (product == null || product.IsDeleted)
            {
                throw new EntityNotFoundException("Sản phẩm", id);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var image in product.ProductImages)
                {
                    var objectName = GetObjectNameFromUrl(image.ImageUrl);
                    if (!string.IsNullOrEmpty(objectName))
                    {
                        await _minioService.RemoveFileAsync(objectName);
                    }
                    
                    productImageRepository.Delete(image);
                }

                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;
                _productRepository.Update(product);
                
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        
        public async Task<ProductImageDto> AddProductImageAsync(Guid productId, AddProductImageDto imageDto)
        {
            var productImageRepository = _unitOfWork.Repository<ProductImage>();
            
            var product = await _productRepository.GetByIdAsync(productId);
            
            if (product == null || product.IsDeleted)
            {
                throw new EntityNotFoundException("Sản phẩm", productId);
            }
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int displayOrder = 0;
                var currentImages = await productImageRepository.Ts
                    .Where(pi => pi.ProductId == productId)
                    .ToListAsync();
                    
                if (currentImages.Any())
                {
                    displayOrder = currentImages.Max(pi => pi.DisplayOrder) + 1;
                }
                
                var fileName = $"products/{productId}/{displayOrder}_{Guid.NewGuid()}{System.IO.Path.GetExtension(imageDto.Image.FileName)}";
                var imageUrl = await _minioService.UploadFileAsync(imageDto.Image, fileName);
                
                if (imageDto.IsMainImage)
                {
                    var mainImages = currentImages.Where(img => img.IsMainImage).ToList();
                    foreach (var mainImage in mainImages)
                    {
                        mainImage.IsMainImage = false;
                        productImageRepository.Update(mainImage);
                    }
                }
                
                var productImage = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    ImageAltText = imageDto.ImageAltText ?? product.ProductName,
                    IsMainImage = imageDto.IsMainImage,
                    DisplayOrder = displayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                
                productImageRepository.Add(productImage);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return new ProductImageDto
                {
                    Id = productImage.Id,
                    ProductId = productImage.ProductId,
                    ImageUrl = productImage.ImageUrl,
                    ImageAltText = productImage.ImageAltText,
                    IsMainImage = productImage.IsMainImage,
                    DisplayOrder = productImage.DisplayOrder
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        
        public async Task<bool> DeleteProductImageAsync(Guid imageId)
        {
            var productImageRepository = _unitOfWork.Repository<ProductImage>();
            
            var image = await productImageRepository.GetByIdAsync(imageId);
            
            if (image == null)
            {
                throw new EntityNotFoundException("Hình ảnh sản phẩm", imageId);
            }
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var objectName = GetObjectNameFromUrl(image.ImageUrl);
                if (!string.IsNullOrEmpty(objectName))
                {
                    await _minioService.RemoveFileAsync(objectName);
                }
                
                productImageRepository.Delete(image);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        
        public async Task<bool> SetMainProductImageAsync(Guid imageId)
        {
            var productImageRepository = _unitOfWork.Repository<ProductImage>();
            
            var image = await productImageRepository.GetByIdAsync(imageId);
            
            if (image == null)
            {
                throw new EntityNotFoundException("Hình ảnh sản phẩm", imageId);
            }
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var otherImages = await productImageRepository.Ts
                    .Where(pi => pi.ProductId == image.ProductId)
                    .ToListAsync();
                    
                foreach (var otherImage in otherImages)
                {
                    otherImage.IsMainImage = (otherImage.Id == imageId);
                    productImageRepository.Update(otherImage);
                }
                
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        
        private string GetObjectNameFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;

                var uri = new Uri(url);
                var segments = uri.Segments;

                if (segments.Length > 2)
                {
                    return string.Join("", segments.Skip(2));
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetProductImagePresignedUrlAsync(Guid imageId, int expiryMinutes = 30)
        {
            var imageRepository = _unitOfWork.Repository<ProductImage>();
            var image = await imageRepository.Ts
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);
            
            if (image == null)
            {
                throw new EntityNotFoundException("Hình ảnh sản phẩm", imageId);
            }
            
            if (string.IsNullOrEmpty(image.ImageUrl))
            {
                throw new Exception("Sản phẩm này không có hình ảnh");
            }
            
            var objectName = GetObjectNameFromUrl(image.ImageUrl);
            if (string.IsNullOrEmpty(objectName))
            {
                throw new Exception("Không thể trích xuất tên file từ URL");
            }
            
            var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, expiryMinutes);
            return presignedUrl;
        }
        
        public async Task<string> GetProductMainImagePresignedUrlAsync(Guid productId, int expiryMinutes = 30)
        {
            var imageRepository = _unitOfWork.Repository<ProductImage>();
            var mainImage = await imageRepository.Ts
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsMainImage && !i.IsDeleted);
            
            if (mainImage == null)
            {
                throw new EntityNotFoundException("Hình ảnh chính của sản phẩm", productId);
            }
            
            if (string.IsNullOrEmpty(mainImage.ImageUrl))
            {
                throw new Exception("Sản phẩm này không có hình ảnh chính");
            }
            
            var objectName = GetObjectNameFromUrl(mainImage.ImageUrl);
            if (string.IsNullOrEmpty(objectName))
            {
                throw new Exception("Không thể trích xuất tên file từ URL");
            }
            
            var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, expiryMinutes);
            return presignedUrl;
        }
    }
} 
