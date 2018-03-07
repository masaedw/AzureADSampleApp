using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureADSampleApp
{
    internal class Program
    {
        private static string Prompt(string name)
        {
            Console.Write($"{name}: ");
            Console.Out.Flush();
            return Console.ReadLine();
        }

        private static async Task Main(string[] args)
        {
            var tenantId = Prompt("tenant id");
            var applicationId = Prompt("application id");
            var applicaitonKey = Prompt("application key");

            var authContext = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");
            var token = await authContext.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(applicationId, applicaitonKey));
        }
    }
}