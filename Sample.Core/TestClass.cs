using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.Sample
{
    public class TestClass
    {
        public async Task StartAsync(ISecuredTokenManager manager)
        {
            await UseSecureHttpClient(manager);
            await UseSimpleHttpClient();
        }

        private async Task UseSecureHttpClient(ISecuredTokenManager manager)
        {
            var exampleService = new SecureHttpClientExecutionSampleService(manager);

            await exampleService.LoginAsync();
            await exampleService.MakeRequestWithCredentials();
        }

        private async Task UseSimpleHttpClient()
        {
            var exampleService = new SimpleHttpClientSampleService();

            await exampleService.MakeRequest();
        }
    }
}