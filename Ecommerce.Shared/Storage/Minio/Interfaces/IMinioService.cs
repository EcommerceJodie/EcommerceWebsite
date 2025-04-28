using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Storage.Minio.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file, string objectName = null, string contentType = null);
        
        Task<string> GeneratePresignedUploadUrlAsync(string objectName, int? expiryMinutes = null);
        
        Task<string> GeneratePresignedDownloadUrlAsync(string objectName, int? expiryMinutes = null);
        
        Task<bool> RemoveFileAsync(string objectName);
        
        Task<string> GetFileUrlAsync(string objectName);
        
        Task<string> UploadStreamAsync(Stream stream, string objectName, string contentType);
        
        bool IsMinioConnectionActive();
        
        Task<bool> ReapplyBucketPolicyAsync();
        
        Task<string> CopyFileAsync(string sourceObjectName, string destinationObjectName);
    }
} 
