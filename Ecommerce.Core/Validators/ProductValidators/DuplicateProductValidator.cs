using FluentValidation;
using Ecommerce.Core.DTOs;
using System;

namespace Ecommerce.Core.Validators.ProductValidators
{
    public class DuplicateProductValidator : AbstractValidator<DuplicateProductDto>
    {
        public DuplicateProductValidator()
        {
            RuleFor(x => x.SourceProductId)
                .NotEmpty()
                .WithMessage("ID sản phẩm nguồn không được để trống");
                
            // Các trường dưới đây là không bắt buộc
            RuleFor(x => x.NewProductName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.NewProductName))
                .WithMessage("Tên sản phẩm không được vượt quá 200 ký tự");
                
            RuleFor(x => x.NewProductSku)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.NewProductSku))
                .WithMessage("SKU không được vượt quá 50 ký tự");
                
            RuleFor(x => x.NewProductSlug)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.NewProductSlug))
                .WithMessage("Slug không được vượt quá 200 ký tự");
        }
    }
} 