$(document).ready(function() {
    // Kiểm tra nếu cần cập nhật giỏ hàng sau khi đăng nhập/đăng ký
    if (window.tempData && window.tempData.updateCart) {
        updateCartItemCount();
    }
    
    // Xử lý thêm vào giỏ hàng từ danh sách sản phẩm
    $(document).on('click', '.add-to-cart', function(e) {
        e.preventDefault();
        
        var productId = $(this).data('product-id');
        var quantity = 1; // Mặc định thêm 1 sản phẩm
        
        addToCart(productId, quantity);
    });
    
    // Xử lý thêm vào giỏ hàng từ trang chi tiết sản phẩm
    $(document).on('click', '.add-to-cart-btn', function(e) {
        e.preventDefault();
        
        var productId = $(this).data('product-id');
        var quantity = parseInt($('#quantity').val());
        
        if (quantity <= 0) {
            showNotification('Số lượng phải lớn hơn 0', 'danger');
            return;
        }
        
        addToCart(productId, quantity);
    });
    
    // Hàm thêm sản phẩm vào giỏ hàng
    function addToCart(productId, quantity) {
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function(result) {
                showNotification('Sản phẩm đã được thêm vào giỏ hàng', 'success');
                updateCartItemCount();
            },
            error: function(xhr, status, error) {
                showNotification('Có lỗi xảy ra: ' + error, 'danger');
            }
        });
    }
    
    // Hiển thị thông báo
    function showNotification(message, type) {
        // Tạo một toast notification
        var toast = $('<div class="toast align-items-center text-white bg-' + type + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
            '<div class="d-flex">' +
            '<div class="toast-body">' + message + '</div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
            '</div>' +
            '</div>');
        
        // Thêm toast vào container
        if ($('.toast-container').length === 0) {
            $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3"></div>');
        }
        $('.toast-container').append(toast);
        
        // Hiển thị toast
        var bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        // Xóa toast sau khi ẩn
        toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
    
    // Cập nhật số lượng sản phẩm trong giỏ hàng
    function updateCartItemCount() {
        $.ajax({
            url: '/Cart/GetCartItemCount',
            type: 'GET',
            success: function(result) {
                // Tìm badge số lượng và cập nhật giá trị
                var badge = $('.cart-icon .badge');
                if (badge.length > 0) {
                    badge.text(result.count);
                    if (result.count > 0) {
                        badge.show();
                    } else {
                        badge.hide();
                    }
                } else if (result.count > 0) {
                    // Nếu chưa có badge, tạo mới
                    $('.cart-icon a').append('<span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">' + 
                        result.count + '<span class="visually-hidden">sản phẩm trong giỏ hàng</span></span>');
                }
            }
        });
    }
}); 