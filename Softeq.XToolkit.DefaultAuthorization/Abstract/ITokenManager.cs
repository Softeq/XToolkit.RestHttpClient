using System.Threading.Tasks;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    //TODO: need to hide internally Token/RefreshToken
    public interface ITokenManager
    {
        string Token { get; }
        string RefreshToken { get; }
        Task SaveTokensAsync(string token, string refreshToken);
        void ResetTokens();
    }
}