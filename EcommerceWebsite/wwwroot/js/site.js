// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Hàm thêm sản phẩm vào giỏ hàng
function addToCart(productId, quantity) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: {
            productId: productId,
            quantity: quantity || 1
        },
        success: function (result) {
            updateCartItemCount();
        },
        error: function (xhr, status, error) {
            showNotification('Lỗi: ' + (xhr.responseJSON?.message || error), 'danger');
        }
    });
}

// Hiển thị thông báo
function showNotification(message, type) {
    // Kiểm tra nếu toast-container chưa tồn tại thì tạo mới
    if ($('.toast-container').length === 0) {
        $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    // Tạo toast
    var toast = $('<div class="toast align-items-center text-white bg-' + type + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
        '<div class="d-flex">' +
        '<div class="toast-body">' + message + '</div>' +
        '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
        '</div>' +
        '</div>');

    // Thêm toast vào container
    $('.toast-container').append(toast);

    // Hiển thị toast
    var bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    // Xóa toast sau khi ẩn
    toast.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// Cập nhật số lượng sản phẩm trong giỏ hàng
function updateCartItemCount() {
    $.ajax({
        url: '/Cart/GetCartItemCount',
        type: 'GET',
        success: function (result) {
            var badge = $('.cart-icon .badge');
            if (badge.length > 0) {
                badge.text(result.count);
                if (result.count > 0) {
                    badge.show();
                } else {
                    badge.hide();
                }
            } else if (result.count > 0) {
                $('.cart-icon a').append('<span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">' +
                    result.count + '<span class="visually-hidden">sản phẩm trong giỏ hàng</span></span>');
            }
        }
    });
}
