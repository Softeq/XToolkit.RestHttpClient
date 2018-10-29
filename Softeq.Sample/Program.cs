using System;
using System.Threading.Tasks;

namespace Softeq.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var root = new Program();

            root.UseSecureHttpClient();
            root.UseSimpleHttpClient();

            Console.ReadLine();
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