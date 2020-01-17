// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;

namespace Softeq.XToolkit.CrossCutting.Content
{
    public interface IHttpContentProvider
    {
        HttpContent GetContent();
    }
}