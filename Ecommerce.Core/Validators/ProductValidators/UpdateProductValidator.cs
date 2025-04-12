using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using FluentValidation;

namespace Ecommerce.Core.Validators.ProductValidators
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UpdateProductValidator(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID sản phẩm là bắt buộc");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Tên sản phẩm là bắt buộc")
                .MaximumLength(200).WithMessage("Tên sản phẩm không được vượt quá 200 ký tự");

            RuleFor(x => x.ProductSlug)
                .NotEmpty().WithMessage("Slug sản phẩm là bắt buộc")
                .MaximumLength(200).WithMessage("Slug sản phẩm không được vượt quá 200 ký tự")
                .MustAsync(async (model, slug, cancellation) => await _productRepository.IsProductSlugUniqueAsync(slug, model.Id))
                .WithMessage("Slug sản phẩm đã tồn tại, vui lòng chọn slug khác");

            RuleFor(x => x.ProductDescription)
                .MaximumLength(5000).WithMessage("Mô tả sản phẩm không được vượt quá 5000 ký tự");

            RuleFor(x => x.ProductPrice)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");

            RuleFor(x => x.ProductDiscountPrice)
                .GreaterThan(0).WithMessage("Giá khuyến mãi phải lớn hơn 0")
                .LessThan(x => x.ProductPrice).WithMessage("Giá khuyến mãi phải nhỏ hơn giá gốc")
                .When(x => x.ProductDiscountPrice.HasValue);

            RuleFor(x => x.ProductStock)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho không được âm");

            RuleFor(x => x.ProductSku)
                .NotEmpty().WithMessage("Mã SKU là bắt buộc")
                .MaximumLength(50).WithMessage("Mã SKU không được vượt quá 50 ký tự")
                .MustAsync(async (model, sku, cancellation) => await _productRepository.IsProductSkuUniqueAsync(sku, model.Id))
                .WithMessage("Mã SKU đã tồn tại, vui lòng chọn mã khác");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Danh mục sản phẩm là bắt buộc")
                .MustAsync(async (categoryId, cancellation) => {
                    var category = await _categoryRepository.GetByIdAsync(categoryId);
                    return category != null && !category.IsDeleted && category.IsActive;
                })
                .WithMessage("Danh mục sản phẩm không tồn tại hoặc không hoạt động");

            RuleFor(x => x.MetaTitle)
                .MaximumLength(100).WithMessage("Tiêu đề Meta không được vượt quá 100 ký tự");

            RuleFor(x => x.MetaDescription)
                .MaximumLength(200).WithMessage("Mô tả Meta không được vượt quá 200 ký tự");
        }
    }
} 