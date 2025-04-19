using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using FluentValidation;

namespace Ecommerce.Core.Validators.CategoryValidators
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID danh mục là bắt buộc");

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Tên danh mục là bắt buộc")
                .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");

            RuleFor(x => x.CategorySlug)
                .NotEmpty().WithMessage("Slug danh mục là bắt buộc")
                .MaximumLength(100).WithMessage("Slug danh mục không được vượt quá 100 ký tự")
                .MustAsync(async (model, slug, cancellation) => 
                    await _categoryRepository.IsCategorySlugUniqueAsync(slug, model.Id))
                .WithMessage("Slug danh mục đã tồn tại, vui lòng chọn slug khác");

            RuleFor(x => x.CategoryDescription)
                .MaximumLength(500).WithMessage("Mô tả danh mục không được vượt quá 500 ký tự");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải là số không âm");
        }
    }
} 
