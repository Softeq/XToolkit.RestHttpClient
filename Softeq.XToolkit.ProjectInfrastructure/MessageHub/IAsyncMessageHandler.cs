using System.Threading.Tasks;

namespace Softeq.XToolkit.CrossCutting.MessageHub
{
    public interface IAsyncMessageHandler<in T>
    {
        Task HandleAsync(T message);
    }
}