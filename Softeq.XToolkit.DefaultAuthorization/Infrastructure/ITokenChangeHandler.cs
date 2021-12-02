namespace Softeq.XToolkit.DefaultAuthorization.Infrastructure
{
    public interface ITokenChangeHandler
    {
        void OnTokenChanged(string token);
    }
}
