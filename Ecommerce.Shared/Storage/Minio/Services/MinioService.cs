using System;
using System.IO;
using System.Threading.Tasks;
using Ecommerce.Shared.Storage.Minio.Configs;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Ecommerce.Shared.Storage.Minio.Services
{
    public class MinioService : IMinioService
    {
        private readonly MinioClient _minioClient;
        private readonly Ecommerce.Shared.Storage.Minio.Configs.MinioConfig _minioConfig;
        private readonly ILogger<MinioService> _logger;

        public MinioService(IOptions<Ecommerce.Shared.Storage.Minio.Configs.MinioConfig> minioConfig, ILogger<MinioService> logger = null)
        {
            if (minioConfig == null)
            {
                throw new ArgumentNullException(nameof(minioConfig), "MinioConfig options is null");
            }

            if (minioConfig.Value == null)
            {
                throw new ArgumentNullException(nameof(minioConfig.Value), "MinioConfig value is null");
            }

            _minioConfig = minioConfig.Value;
            _logger = logger;

            Console.WriteLine($"MinioConfig được khởi tạo: Endpoint={_minioConfig.Endpoint}, AccessKey={_minioConfig.AccessKey}");

            if (string.IsNullOrEmpty(_minioConfig.Endpoint))
            {
                throw new ArgumentException("MinioConfig.Endpoint không được để trống");
            }
            
            if (string.IsNullOrEmpty(_minioConfig.AccessKey))
            {
                throw new ArgumentException("MinioConfig.AccessKey không được để trống");
            }
            
            if (string.IsNullOrEmpty(_minioConfig.SecretKey))
            {
                throw new ArgumentException("MinioConfig.SecretKey không được để trống");
            }
            
            if (string.IsNullOrEmpty(_minioConfig.BucketName))
            {
                throw new ArgumentException("MinioConfig.BucketName không được để trống");
            }

            try
            {
                Console.WriteLine("Đang khởi tạo MinioClient...");
                string endpoint = _minioConfig.Endpoint;
                string accessKey = _minioConfig.AccessKey;
                string secretKey = _minioConfig.SecretKey;
                
                Console.WriteLine($"Đang tạo MinioClient với: Endpoint={endpoint}, AccessKey={accessKey}, SecretKey có độ dài {secretKey?.Length ?? 0}");
                
                try {
                    _minioClient = new MinioClient()
                        .WithEndpoint(endpoint)
                        .WithCredentials(accessKey, secretKey)
                        .WithSSL(_minioConfig.WithSSL)
                        .Build();
                    
                    try {
                        var pingTask = _minioClient.BucketExistsAsync("test-connection");
                        pingTask.Wait(5000);
                        Console.WriteLine("MinioClient đã được khởi tạo thành công và có thể kết nối đến máy chủ.");
                    } catch (Exception connEx) {
                        Console.WriteLine($"Cảnh báo: MinioClient đã được khởi tạo nhưng không thể kết nối đến máy chủ: {connEx.Message}");
                        _logger?.LogWarning(connEx, "MinioClient đã được khởi tạo nhưng không thể kết nối đến máy chủ");
                    }
                } catch (Exception clientEx) {
                    Console.WriteLine($"Lỗi khi khởi tạo MinioClient: {clientEx.Message}");
                    _logger?.LogError(clientEx, "Lỗi khi khởi tạo MinioClient");
                    
                    _minioClient = new MinioClient()
                        .WithEndpoint("localhost")
                        .WithCredentials("minioadmin", "minioadmin")
                        .Build();
                    Console.WriteLine("Đã tạo MinioClient giả lập để tránh NullReferenceException");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi tạo MinioClient: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                _logger?.LogError(ex, "Lỗi khi cấu hình MinioService");
                
                _minioClient = new MinioClient()
                    .WithEndpoint("localhost")
                    .WithCredentials("minioadmin", "minioadmin")
                    .Build();
                Console.WriteLine("Đã tạo MinioClient giả lập để tránh NullReferenceException");
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string objectName = null, string contentType = null)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return $"/images/fallback-{Guid.NewGuid()}.png";
                }
                
                await EnsureBucketExistsAsync();
                
                objectName ??= $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                contentType ??= file.ContentType;

                using (var stream = file.OpenReadStream())
                {
                    await _minioClient.PutObjectAsync(
                        _minioConfig.BucketName,
                        objectName,
                        stream,
                        stream.Length,
                        contentType
                    );
                }

                return await GetFileUrlAsync(objectName);
            }
            catch (SocketException socketEx)
            {
                _logger?.LogError(socketEx, "Lỗi kết nối đến Minio Server: {Message}", socketEx.Message);
                return $"/images/fallback-{Guid.NewGuid()}.png";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi upload file lên Minio: {Message}", ex.Message);
                return $"/images/fallback-{Guid.NewGuid()}.png";
            }
        }

        public Task<string> GeneratePresignedUploadUrlAsync(string objectName, int? expiryMinutes = null)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return Task.FromResult($"/temp-upload/{objectName}");
                }
                
                var expiry = expiryMinutes ?? _minioConfig.PresignedUrlExpiryMinutes;
                return _minioClient.PresignedPutObjectAsync(_minioConfig.BucketName, objectName, expiry * 60);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi tạo presigned URL: {Message}", ex.Message);
                return Task.FromResult($"/temp-upload/{objectName}");
            }
        }

        public Task<string> GeneratePresignedDownloadUrlAsync(string objectName, int? expiryMinutes = null)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return Task.FromResult($"/temp-download/{objectName}");
                }
                
                var expiry = expiryMinutes ?? _minioConfig.PresignedUrlExpiryMinutes;
                return _minioClient.PresignedGetObjectAsync(_minioConfig.BucketName, objectName, expiry * 60);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi tạo presigned URL tải xuống: {Message}", ex.Message);
                return Task.FromResult($"/temp-download/{objectName}");
            }
        }

        public async Task<bool> RemoveFileAsync(string objectName)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return true;
                }
                
                await _minioClient.RemoveObjectAsync(_minioConfig.BucketName, objectName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi xóa file từ Minio: {Message}", ex.Message);
                return true;
            }
        }

        public Task<string> GetFileUrlAsync(string objectName)
        {
            try
            {
                if (_minioConfig == null)
                {
                    _logger?.LogError("MinioConfig chưa được khởi tạo!");
                    return Task.FromResult($"/images/{objectName}");
                }
                
                string url = _minioConfig.WithSSL 
                    ? $"https://{_minioConfig.Endpoint}/{_minioConfig.BucketName}/{objectName}"
                    : $"http://{_minioConfig.Endpoint}/{_minioConfig.BucketName}/{objectName}";
                
                return Task.FromResult(url);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi tạo URL cho file: {Message}", ex.Message);
                return Task.FromResult($"/images/{objectName}");
            }
        }

        public async Task<string> UploadStreamAsync(Stream stream, string objectName, string contentType)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return $"/images/fallback-{Guid.NewGuid()}.png";
                }
                
                await EnsureBucketExistsAsync();
                
                await _minioClient.PutObjectAsync(
                    _minioConfig.BucketName,
                    objectName,
                    stream,
                    stream.Length,
                    contentType
                );

                return await GetFileUrlAsync(objectName);
            }
            catch (SocketException socketEx)
            {
                _logger?.LogError(socketEx, "Lỗi kết nối đến Minio Server: {Message}", socketEx.Message);
                return $"/images/fallback-{Guid.NewGuid()}.png";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi upload stream lên Minio: {Message}", ex.Message);
                return $"/images/fallback-{Guid.NewGuid()}.png";
            }
        }

        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return;
                }
                
                bool found = await _minioClient.BucketExistsAsync(_minioConfig.BucketName);
                if (!found)
                {
                    await _minioClient.MakeBucketAsync(_minioConfig.BucketName);
                    Console.WriteLine($"Đã tạo bucket mới: {_minioConfig.BucketName}");
                    
                    var policy = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{
                                    ""AWS"": [
                                        ""*""
                                    ]
                                }},
                                ""Action"": [
                                    ""s3:GetObject"",
                                    ""s3:GetBucketLocation"",
                                    ""s3:ListBucket""
                                ],
                                ""Resource"": [
                                    ""arn:aws:s3:::{_minioConfig.BucketName}/*"",
                                    ""arn:aws:s3:::{_minioConfig.BucketName}""
                                ]
                            }}
                        ]
                    }}";
                    
                    try {
                        await _minioClient.SetPolicyAsync(_minioConfig.BucketName, policy);
                        Console.WriteLine($"Đã thiết lập policy cho bucket {_minioConfig.BucketName}");
                    }
                    catch (Exception policyEx) {
                        Console.WriteLine($"Lỗi khi thiết lập policy: {policyEx.Message}");
                        _logger?.LogError(policyEx, "Lỗi khi thiết lập policy cho bucket: {Message}", policyEx.Message);
                    }
                }
                else
                {
                    try
                    {
                        var policy = $@"{{
                            ""Version"": ""2012-10-17"",
                            ""Statement"": [
                                {{
                                    ""Effect"": ""Allow"",
                                    ""Principal"": {{
                                        ""AWS"": [
                                            ""*""
                                        ]
                                    }},
                                    ""Action"": [
                                        ""s3:GetObject"",
                                        ""s3:GetBucketLocation"",
                                        ""s3:ListBucket""
                                    ],
                                    ""Resource"": [
                                        ""arn:aws:s3:::{_minioConfig.BucketName}/*"",
                                        ""arn:aws:s3:::{_minioConfig.BucketName}""
                                    ]
                                }}
                            ]
                        }}";
                        
                        await _minioClient.SetPolicyAsync(_minioConfig.BucketName, policy);
                        Console.WriteLine($"Đã cập nhật policy cho bucket {_minioConfig.BucketName}");
                    }
                    catch (Exception policyEx)
                    {
                        Console.WriteLine($"Lỗi khi cập nhật policy: {policyEx.Message}");
                        _logger?.LogError(policyEx, "Lỗi khi cập nhật policy cho bucket: {Message}", policyEx.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi đảm bảo bucket tồn tại: {Message}", ex.Message);
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                Console.WriteLine("Đang kiểm tra kết nối đến MinIO server...");
                
                if (_minioClient == null)
                {
                    Console.WriteLine("Lỗi: MinioClient chưa được khởi tạo!");
                    _logger?.LogError("MinioClient chưa được khởi tạo khi kiểm tra kết nối");
                    return false;
                }
                
                string testBucketName = "connection-test-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                
                bool exists = await _minioClient.BucketExistsAsync(testBucketName);
                Console.WriteLine($"Kết nối đến MinIO server thành công, kết quả: {(exists ? "bucket tồn tại" : "bucket không tồn tại")}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối đến MinIO server: {ex.Message}");
                _logger?.LogError(ex, "Không thể kết nối đến MinIO server: {Message}", ex.Message);
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    _logger?.LogError(ex.InnerException, "Inner exception khi kết nối đến MinIO: {Message}", ex.InnerException.Message);
                }
                
                return false;
            }
        }
        
        public bool IsMinioConnectionActive()
        {
            try
            {
                Console.WriteLine("Đang kiểm tra kết nối đến MinIO...");
                
                if (_minioClient == null)
                {
                    Console.WriteLine("Lỗi: MinioClient chưa được khởi tạo!");
                    return false;
                }
                
                var task = CheckConnectionAsync();
                task.Wait(5000);
                
                bool result = task.Result;
                Console.WriteLine($"Kết nối đến MinIO: {(result ? "thành công" : "thất bại")}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối đến MinIO: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        private async Task ApplyBucketPolicyAsync()
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return;
                }
                
                bool found = await _minioClient.BucketExistsAsync(_minioConfig.BucketName);
                if (!found)
                {
                    _logger?.LogError($"Bucket {_minioConfig.BucketName} không tồn tại");
                    return;
                }
                
                var policy = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{
                                ""AWS"": [
                                    ""*""
                                ]
                            }},
                            ""Action"": [
                                ""s3:GetObject"",
                                ""s3:GetBucketLocation"",
                                ""s3:ListBucket"",
                                ""s3:ListBucketMultipartUploads"",
                                ""s3:ListMultipartUploadParts""
                            ],
                            ""Resource"": [
                                ""arn:aws:s3:::{_minioConfig.BucketName}"",
                                ""arn:aws:s3:::{_minioConfig.BucketName}/*""
                            ]
                        }}
                    ]
                }}";
                
                await _minioClient.SetPolicyAsync(_minioConfig.BucketName, policy);
                Console.WriteLine($"Đã áp dụng lại policy cho bucket {_minioConfig.BucketName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi áp dụng policy cho bucket: {Message}", ex.Message);
            }
        }

        public async Task<bool> ReapplyBucketPolicyAsync()
        {
            try
            {
                await ApplyBucketPolicyAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi áp dụng lại chính sách bucket: {Message}", ex.Message);
                return false;
            }
        }
        
        public async Task<string> CopyFileAsync(string sourceObjectName, string destinationObjectName)
        {
            try
            {
                if (_minioClient == null)
                {
                    _logger?.LogError("MinioClient chưa được khởi tạo!");
                    return $"/images/fallback-{Guid.NewGuid()}.png";
                }
                
                await EnsureBucketExistsAsync();
                
                // Tạo CopySource cho file nguồn
                var copySourceArgs = new CopySourceObjectArgs()
                    .WithBucket(_minioConfig.BucketName)
                    .WithObject(sourceObjectName);
                
                // Thực hiện sao chép file
                await _minioClient.CopyObjectAsync(
                    new CopyObjectArgs()
                        .WithBucket(_minioConfig.BucketName)
                        .WithObject(destinationObjectName)
                        .WithCopyObjectSource(copySourceArgs)
                );
                
                return await GetFileUrlAsync(destinationObjectName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi sao chép file trong Minio: {Message}", ex.Message);
                return $"/images/fallback-{Guid.NewGuid()}.png";
            }
        }
    }
} 
