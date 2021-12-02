
using System;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    public interface ISecuredTokenManager
    {
        event EventHandler<string> TokenChanged;

        string Token { get; }
        bool IsTokenExpired { get; }
        string RefreshToken { get; }
        void SaveTokens(string token, string refreshToken, int tokenExpirationTimespanInSeconds);
        void ResetTokens();
    }
}