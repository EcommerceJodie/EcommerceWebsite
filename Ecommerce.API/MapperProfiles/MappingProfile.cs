using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models.Entities;
using System.Linq;

namespace Ecommerce.API.MapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl).ToList()));

            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
            
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.CategoryImageUrl, 
                           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CategoryImageUrl) ? string.Empty : src.CategoryImageUrl));
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.CategoryImageUrl, 
                           opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CategoryImageUrl) ? string.Empty : src.CategoryImageUrl));
        }
    }
} 