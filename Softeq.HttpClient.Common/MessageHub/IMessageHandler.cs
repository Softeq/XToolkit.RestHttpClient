namespace Softeq.HttpClient.Common.MessageHub
{
    public interface IMessageHandler<in T>
    {
        void Handle(T message);
    }
}