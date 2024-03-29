// Developed for PAWS-HALO by Softeq Development
// Corporation http://www.softeq.com

using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization.Infrastructure;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    public interface ISessionApiService
    {
        void SetBaseUrl(string baseUrl);
        Task<ExecutionResult<LoginStatus>> LoginAsync(string login, string password);
        Task<ExecutionStatus> RefreshTokenAsync();
        Task<ExecutionResult<ResendEmailStatus>> ResendConfirmationAsync(string email);
        Task<CheckRegistrationStatus> IsAccountAlreadyRegistered(string email);
        void Logout();
        Task<ExecutionResult<RegistrationStatus>> RegisterAccountAsync(string login, string password);
        Task<ExecutionResult<ForgotPasswordStatus>> ForgotPasswordAsync(string login);
    }
}