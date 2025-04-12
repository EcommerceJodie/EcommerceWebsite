using Ecommerce.Shared.Storage.Minio.Configs;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using Ecommerce.Shared.Storage.Minio.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ecommerce.Shared.Storage.Minio
{
    public static class MinioServiceExtension
    {
        public static IServiceCollection AddMinioService(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("Đang cấu hình MinioService...");
                
                var minioConfigSection = configuration.GetSection("MinioConfig");
                if (!minioConfigSection.Exists())
                {
                    Console.WriteLine("Section 'MinioConfig' không tồn tại trong cấu hình");
                    throw new InvalidOperationException("Section 'MinioConfig' is missing in configuration");
                }
                
                Console.WriteLine("Đã tìm thấy section 'MinioConfig'");
                
                Console.WriteLine($"MinioConfig:Endpoint = {minioConfigSection["Endpoint"]}");
                Console.WriteLine($"MinioConfig:AccessKey = {minioConfigSection["AccessKey"]}");
                Console.WriteLine($"MinioConfig:SecretKey = {(minioConfigSection["SecretKey"] != null ? "***" : "<null>")}");
                Console.WriteLine($"MinioConfig:BucketName = {minioConfigSection["BucketName"]}");
                
                var testConfig = minioConfigSection.Get<MinioConfig>();
                if (testConfig == null)
                {
                    Console.WriteLine("Không thể đọc cấu hình MinioConfig từ section");
                    throw new InvalidOperationException("Failed to bind 'MinioConfig' from configuration");
                }
                
                Console.WriteLine("Đã đọc được cấu hình MinioConfig");
                Console.WriteLine($"Endpoint từ testConfig: {testConfig.Endpoint}");
                Console.WriteLine($"AccessKey từ testConfig: {testConfig.AccessKey}");
                
                if (string.IsNullOrEmpty(testConfig.Endpoint))
                {
                    Console.WriteLine("'MinioConfig:Endpoint' bị thiếu trong cấu hình");
                    throw new InvalidOperationException("'MinioConfig:Endpoint' is missing in configuration");
                }
                
                if (string.IsNullOrEmpty(testConfig.AccessKey))
                {
                    Console.WriteLine("'MinioConfig:AccessKey' bị thiếu trong cấu hình");
                    throw new InvalidOperationException("'MinioConfig:AccessKey' is missing in configuration");
                }
                
                if (string.IsNullOrEmpty(testConfig.SecretKey))
                {
                    Console.WriteLine("'MinioConfig:SecretKey' bị thiếu trong cấu hình");
                    throw new InvalidOperationException("'MinioConfig:SecretKey' is missing in configuration");
                }
                
                if (string.IsNullOrEmpty(testConfig.BucketName))
                {
                    Console.WriteLine("'MinioConfig:BucketName' bị thiếu trong cấu hình");
                    throw new InvalidOperationException("'MinioConfig:BucketName' is missing in configuration");
                }
                
                services.Configure<MinioConfig>(minioConfigSection);
                Console.WriteLine("Đã đăng ký cấu hình MinioConfig thành công");
                
                services.AddScoped<IMinioService, MinioService>();
                Console.WriteLine("Đã đăng ký dịch vụ IMinioService thành công");
                
                return services;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cấu hình MinioService: {ex.Message}");
                throw;
            }
        }
    }
} 