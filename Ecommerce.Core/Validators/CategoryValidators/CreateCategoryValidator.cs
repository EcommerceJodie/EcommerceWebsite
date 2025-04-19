using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using FluentValidation;

namespace Ecommerce.Core.Validators.CategoryValidators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Tên danh mục là bắt buộc")
                .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");

            RuleFor(x => x.CategorySlug)
                .NotEmpty().WithMessage("Slug danh mục là bắt buộc")
                .MaximumLength(100).WithMessage("Slug danh mục không được vượt quá 100 ký tự")
                .MustAsync(async (slug, cancellation) => await _categoryRepository.IsCategorySlugUniqueAsync(slug))
                .WithMessage("Slug danh mục đã tồn tại, vui lòng chọn slug khác");

            RuleFor(x => x.CategoryDescription)
                .MaximumLength(500).WithMessage("Mô tả danh mục không được vượt quá 500 ký tự");

            RuleFor(x => x.CategoryImageUrl)
                .MaximumLength(255).WithMessage("URL hình ảnh không được vượt quá 255 ký tự");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải là số không âm");
        }
    }
} 
