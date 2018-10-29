using System.Threading.Tasks;

namespace Softeq.HttpClient.Common.Executor
{
    public interface IAsyncExecutionContext
    {
        Task ExecuteAgainAsync();
        int ExecutionsCount { get; }
    }
}