using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using RestSharp;

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
            var subscriptioinId = Prompt("Azure Subscription Id");
            var tenantId = Prompt("Azure AD Tenant Id");
            var applicationId = Prompt("Azure AD Application Id");
            var applicaitonKey = Prompt("Azure AD Application Key");
            var workspaceId = Prompt("Log Analytics Workspace Id");
            var workspaceName = Prompt("Log Analytics Workspace Name");
            var resourceGroupName = Prompt("Log Analytics Resource Group Name");

            // URLはAzure ADの場合
            var authContext = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");

            // Azure上のリソースに対する操作の要求
            var token = await authContext.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(applicationId, applicaitonKey));

            // client credential flow の場合は ARM APIを使う必要がある
            // https://dev.loganalytics.io/documentation/Overview/URLs
            var baseUrl = $"https://management.azure.com/subscriptions/{subscriptioinId}/resourceGroups/{resourceGroupName}/providers/Microsoft.OperationalInsights/workspaces/{workspaceName}/api/";
            var client = new RestClient(baseUrl);

            var request = new RestRequest("query", Method.POST);

            // Due to error message, these are the available api-versions:
            //
            // The supported api-versions are '2015-03-20, 2015-11-01-preview, 2017-01-01-preview, 2017-03-03-preview, 2017-03-15-preview, 2017-04-26-preview'.
            // The supported locations are 'eastus, westeurope, southeastasia, australiasoutheast, westcentralus, japaneast, uksouth, centralindia, canadacentral'.
            //
            // and for now, not all of these are documented.
            // https://dev.loganalytics.io/documentation/Overview/API-Version

            request.AddQueryParameter("api-version", "2017-01-01-preview");
            request.AddHeader("Authorization", $"{token.AccessTokenType} {token.AccessToken}");
            request.AddHeader("Prefer", "response-v1=true");
            var query = @"
// What are the 50th, 90th, and 95th percentiles of request duration in the past 24 hours?
ApplicationInsights
| where TelemetryType == ""Request""
| where TimeGenerated >= ago(24h)
| summarize percentiles(RequestDuration, 50, 90, 95) by bin(TimeGenerated, 1h)
| render timechart
";
            request.AddJsonBody(new
            {
                query,
            });

            var result = await client.ExecuteTaskAsync(request);

            Console.WriteLine(result.Content);
        }
    }
}