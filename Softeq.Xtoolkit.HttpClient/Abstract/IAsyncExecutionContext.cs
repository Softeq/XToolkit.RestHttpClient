// Developed for LilBytes by Softeq Development Corporation
//

using System.Threading.Tasks;

namespace Softeq.XToolkit.HttpClient.Abstract
{
    public interface IAsyncExecutionContext
    {
        Task ExecuteAgainAsync();
        int ExecutionsCount { get; }
    }
}