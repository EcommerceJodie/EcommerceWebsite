# Hướng dẫn sử dụng Docker Compose trong dự án

## Cài đặt Docker
Trước khi bắt đầu, bạn cần cài đặt Docker và Docker Compose trên máy tính của mình:
- [Tải Docker cho Windows](https://docs.docker.com/desktop/install/windows-install/)
- [Tải Docker cho Mac](https://docs.docker.com/desktop/install/mac-install/)
- [Tải Docker cho Linux](https://docs.docker.com/desktop/install/linux-install/)

## Chạy MinIO bằng Docker Compose

### Bước 1: Chạy Docker Compose
Mở terminal tại thư mục gốc của dự án và chạy lệnh:

```bash
docker-compose up -d
```

Lệnh này sẽ khởi động container MinIO trong chế độ nền. MinIO sẽ được khởi chạy trên hai cổng:
- Cổng 9000: Dùng cho API (REST/S3)
- Cổng 9001: Dùng cho giao diện quản trị (Console)

### Bước 2: Truy cập MinIO Console
Sau khi khởi động thành công, bạn có thể truy cập giao diện quản trị MinIO tại:
http://localhost:9001

Thông tin đăng nhập:
- Username: minioadmin
- Password: minioadmin

### Bước 3: Tạo bucket
Sau khi đăng nhập vào giao diện quản trị, tạo các bucket cần thiết:
1. Nhấp vào "Create Bucket"
2. Đặt tên bucket là "product-images" và "ecommerce-products"
3. Nhấp vào "Create Bucket"

## Dừng MinIO
Để dừng các container:

```bash
docker-compose down
```

## Lưu ý về cấu hình
Trong ứng dụng, MinIO được cấu hình qua `appsettings.json` với các thông số:
- Endpoint: localhost:9000
- AccessKey: minioadmin
- SecretKey: minioadmin
- BucketName: product-images (hoặc ecommerce-products tùy theo ứng dụng)

## Xử lý sự cố
Nếu gặp lỗi "S3 API Requests must be made to API port", hãy đảm bảo:
1. MinIO container đang chạy
2. Endpoint trong cấu hình đang trỏ đến cổng 9000 (không phải 9001)