using System.Threading.Tasks;

namespace Softeq.XToolkit.CrossCutting.Executor
{
    public interface IAsyncExecutionContext
    {
        Task ExecuteAgainAsync();
        int ExecutionsCount { get; }
    }
}