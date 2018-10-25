using System;
using System.Collections.Generic;
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
            try
            {
                var customService = new CustomService();

                await customService.LoginAsync();
                await customService.TestMethodHighPriority();
                await customService.RestoreTokenAsync();
                await customService.TestMethodHighPriority();
            }
            catch (Exception ex)
            {

            }
            Console.WriteLine("FINISHED");
        }
    }
}