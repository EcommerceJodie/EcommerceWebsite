using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Ecommerce.Core.MapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            

            CreateMap<MenuConfig, MenuConfigDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.Category.CategorySlug))
                .ForMember(dest => dest.CategoryImageUrl, opt => opt.MapFrom(src => src.Category.CategoryImageUrl))
                .ForMember(dest => dest.Children, opt => opt.Ignore());
            
            CreateMap<CreateMenuConfigDto, MenuConfig>();
            CreateMap<UpdateMenuConfigDto, MenuConfig>();


            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl).ToList()))
                .ForMember(dest => dest.ImageUrlsWithDetails, opt => opt.MapFrom(src => src.ProductImages));
            

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
            
            CreateMap<UpdateProductDto, Product>();
            

            CreateMap<IFormFile, ProductImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ImageAltText, opt => opt.Ignore())
                .ForMember(dest => dest.IsMainImage, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            

            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<AddProductImageDto, ProductImage>();
        }
    }
} 
