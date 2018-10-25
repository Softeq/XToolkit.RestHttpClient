// Developed for LilBytes by Softeq Development Corporation
//

using System.Threading.Tasks;

namespace Softeq.Sample
{
    public interface ISecureStorage
    {
        Task<string> GetAsync(string key);
        void Remove(string key);
        Task AddAsync(string key, string value);
    }
}