using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.Sample
{
    public class MembershipService : ITokenManager
    {
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";

        private readonly ISecureStorage _secureStorage;

        public string Token { get; private set; }

        public string RefreshToken { get; private set; }

        public MembershipService(ISecureStorage secureStorage)
        {
            _secureStorage = secureStorage;
        }

        public void ResetTokens()
        {
            _secureStorage.Remove(AccessTokenKey);
            _secureStorage.Remove(RefreshTokenKey);
        }

        public async Task SaveTokensAsync(string token, string refreshToken)
        {
            ResetTokens();

            Token = token;
            RefreshToken = refreshToken;

            await _secureStorage.AddAsync(AccessTokenKey, token).ConfigureAwait(false);
            await _secureStorage.AddAsync(RefreshTokenKey, refreshToken).ConfigureAwait(false);
        }
    }
}