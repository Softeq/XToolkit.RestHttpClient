using System.Threading.Tasks;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    //TODO: need to hide internally Token/RefreshToken
    public interface IMembershipService
    {
        string Token { get; }
        string RefreshToken { get; }
        Task SaveTokensAsync(string token, string refreshToken);
        void ResetTokens();
    }
}