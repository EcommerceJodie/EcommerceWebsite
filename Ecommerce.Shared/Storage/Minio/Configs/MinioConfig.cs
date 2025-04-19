using System;

namespace Ecommerce.Shared.Storage.Minio.Configs
{
    public class MinioConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool WithSSL { get; set; }
        public string BucketName { get; set; }
        public int PresignedUrlExpiryMinutes { get; set; } = 60;
    }
} 
