using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models.Entities;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.API.MapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl).ToList()));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
                
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
            
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
            
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.CategoryImageUrl, 
                           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CategoryImageUrl) ? string.Empty : src.CategoryImageUrl));
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.CategoryImageUrl, 
                           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CategoryImageUrl) ? string.Empty : src.CategoryImageUrl));
                           

            CreateMap<MenuConfig, MenuConfigDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.Category.CategorySlug))
                .ForMember(dest => dest.CategoryImageUrl, opt => opt.MapFrom(src => src.Category.CategoryImageUrl))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children
                    .Where(c => !c.IsDeleted && c.IsVisible)
                    .ToList()));
                
            CreateMap<CreateMenuConfigDto, MenuConfig>();
            CreateMap<UpdateMenuConfigDto, MenuConfig>();
        }
    }
} 
