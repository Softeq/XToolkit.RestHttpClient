using System;
using System.Threading.Tasks;

namespace Softeq.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => new Program().Create());
            Console.ReadLine();
        }

        public async Task Create()
        {
            var exampleService = new ExampleService();

            await exampleService.LoginAsync();
            await exampleService.TestMethodHighPriority();
            await exampleService.RestoreTokenAsync();
            await exampleService.TestMethodHighPriority();
            Console.WriteLine("FINISHED");
        }
    }
}