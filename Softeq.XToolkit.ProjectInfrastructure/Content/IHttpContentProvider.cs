using System.Net.Http;

namespace Softeq.XToolkit.CrossCutting.Content
{
    public interface IHttpContentProvider
    {
        HttpContent GetContent();
    }
}