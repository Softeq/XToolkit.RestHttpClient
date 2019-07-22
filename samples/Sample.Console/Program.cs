using System.Threading.Tasks;

namespace Sample.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var root = new Program();

            await root.UseSecureHttpClient();
            await root.UseSimpleHttpClient();

            System.Console.ReadLine();
        }

        public async Task UseSecureHttpClient()
        {
            var exampleService = new SecureHttpClientExecutionSampleService();

            await exampleService.LoginAsync();
            await exampleService.MakeRequestWithCredentials();
        }

        public async Task UseSimpleHttpClient()
        {
            var exampleService = new SimpleHttpClientSampleService();

            await exampleService.MakeRequest();
        }
    }
}