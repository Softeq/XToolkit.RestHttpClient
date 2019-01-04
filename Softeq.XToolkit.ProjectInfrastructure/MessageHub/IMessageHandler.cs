namespace Softeq.XToolkit.CrossCutting.MessageHub
{
    public interface IMessageHandler<in T>
    {
        void Handle(T message);
    }
}