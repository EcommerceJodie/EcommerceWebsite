using System;
using System.Threading.Tasks;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface IRedisRepository
    {
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string> GetAsync(string key);
        Task<bool> DeleteAsync(string key);
        Task<bool> KeyExistsAsync(string key);
    }
} 
