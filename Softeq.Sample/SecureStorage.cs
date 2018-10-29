using System.Collections.Generic;
using System.Threading.Tasks;

namespace Softeq.Sample
{
    public class SecureStorage : ISecureStorage
    {
        private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        public Task AddAsync(string key, string value)
        {
            return Task.Run(() => _dictionary.Add(key, value));
        }

        public Task<string> GetAsync(string key)
        {
            _dictionary.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public void Remove(string key)
        {
            _dictionary.Remove(key);
        }
    }
}
