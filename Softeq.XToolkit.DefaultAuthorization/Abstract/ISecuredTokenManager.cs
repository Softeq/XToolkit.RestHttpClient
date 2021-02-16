
namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    public interface ISecuredTokenManager
    {
        string Token { get; }
        bool IsTokenExpired { get; }
        string RefreshToken { get; }
        void SaveTokens(string token, string refreshToken, int tokenExpirationTimespanInSeconds);
        void ResetTokens();
    }
}