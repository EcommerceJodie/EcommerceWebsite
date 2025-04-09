using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var productRepository = _unitOfWork.Repository<Product>();
            
            var products = await productRepository.GetAll()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var productRepository = _unitOfWork.Repository<Product>();
            
            var product = await productRepository.GetAll()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var productRepository = _unitOfWork.Repository<Product>();
            
            var products = await productRepository.GetAll()
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<List<ProductDto>> GetFeaturedProductsAsync()
        {
            var productRepository = _unitOfWork.Repository<Product>();
            
            var products = await productRepository.GetAll()
                .Where(p => p.IsFeatured)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var productRepository = _unitOfWork.Repository<Product>();
                await productRepository.AddAsync(product);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                
                return _mapper.Map<ProductDto>(product);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto)
        {
            var productRepository = _unitOfWork.Repository<Product>();
            var existingProduct = await productRepository.GetByIdAsync(productDto.Id);
            
            if (existingProduct == null)
                return null;

            _mapper.Map(productDto, existingProduct);
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                productRepository.Update(existingProduct);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                
                return _mapper.Map<ProductDto>(existingProduct);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var productRepository = _unitOfWork.Repository<Product>();
            var product = await productRepository.GetByIdAsync(id);
            
            if (product == null)
                return false;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                productRepository.Delete(product);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
} 