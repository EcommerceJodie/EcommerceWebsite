using System;

namespace Ecommerce.Core.DTOs
{
    public class DuplicateProductDto
    {
        /// <summary>
        /// ID của sản phẩm gốc cần nhân bản
        /// </summary>
        public Guid SourceProductId { get; set; }
        
        /// <summary>
        /// Tên mới cho sản phẩm được nhân bản (nếu không cung cấp sẽ tự động thêm "- Copy" vào tên cũ)
        /// </summary>
        public string NewProductName { get; set; }
        
        /// <summary>
        /// SKU mới cho sản phẩm (nếu không cung cấp sẽ tự động thêm "-COPY" vào SKU cũ)
        /// </summary>
        public string NewProductSku { get; set; }
        
        /// <summary>
        /// Slug mới cho sản phẩm (nếu không cung cấp sẽ tự động thêm "-copy" vào slug cũ)
        /// </summary>
        public string NewProductSlug { get; set; }
        
        /// <summary>
        /// Có sao chép hình ảnh hay không
        /// </summary>
        public bool CopyImages { get; set; } = true;
    }
} 