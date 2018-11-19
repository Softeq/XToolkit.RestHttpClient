using System.Threading.Tasks;

namespace Softeq.HttpClient.Common.MessageHub
{
    public interface IAsyncMessageHandler<in T>
    {
        Task HandleAsync(T message);
    }
}