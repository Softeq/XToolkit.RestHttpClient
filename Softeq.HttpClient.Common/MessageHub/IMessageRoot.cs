using System.Threading.Tasks;

namespace Softeq.HttpClient.Common.MessageHub
{
    public interface IMessageRoot
    {
        void Subscribe<T>(IMessageHandler<T> messageHandler);
        void Subscribe<T>(IAsyncMessageHandler<T> messageHandler);

        void Unsubscribe<T>(IMessageHandler<T> messageHandler);
        void Unsubscribe<T>(IAsyncMessageHandler<T> messageHandler);

        void Raise<T>(T evnt);
        Task RaiseAsync<T>(T evnt);
    }
}