using System.Threading.Tasks;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    public interface ISecuredTokenManager
    {
        string Token { get; }
        string RefreshToken { get; }
        bool AreTokensMarkedForDeletion { get; }
        Task SaveTokensAsync(string token, string refreshToken);
        Task ResetTokensAsync(bool shouldDeleteTokensPermanently = true);
    }
}