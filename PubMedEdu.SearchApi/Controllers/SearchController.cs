using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using PubMedEdu.SearchApi.Models;

namespace PubMedEdu.SearchApi.Controllers;

[Controller]
[Route("/api/v1/search")]
public class SearchController(ILogger<SearchController> logger, IConfiguration configuration) : ControllerBase
{
    private const string MlModelEndpointUri = "https://localhost";

    [HttpPost]
    public async Task<IActionResult> ExecuteSearch([FromBody] SearchRequest request)
    {
        (HttpStatusCode statusCode, SearchResult? result) searchResult = await GetSearchResult(request);

        if (searchResult.statusCode is not HttpStatusCode.OK)
        {
            return StatusCode((int) searchResult.statusCode, "ML Service Error");
        }

        if (searchResult.result is null)
        {
            return StatusCode(500, "ML result is null");
        }
        
        await TrySendHistory(request.Prompt, searchResult.result);

        return Ok(searchResult.result);
    }

    private async Task<(HttpStatusCode, SearchResult?)> GetSearchResult(SearchRequest request)
    {
        // TODO: For testing only.
        await Task.Delay(2000);

        return (HttpStatusCode.OK, new SearchResult {PossibleResult = "Ligma"});
        
        using var httpClient = new HttpClient();
        
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(MlModelEndpointUri, request);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"ML Endpoint did not return valid status code: {response.StatusCode}");

            return (response.StatusCode, null);
        }
        
        SearchResult mlResult = await response.Content.ReadFromJsonAsync<SearchResult>()
                                ?? throw new InvalidOperationException("Content of parsed ML Response cannot be null");

        return (response.StatusCode, mlResult);
    }

    private async Task TrySendHistory(string prompt, SearchResult resultObject)
    {
        try
        {
            string? userId = User.FindFirst("oid")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var history = new SearchHistoryMessage
                {
                    Prompt = prompt,
                    Results = resultObject,
                    UserId = userId
                };
                
                string? connectionString = configuration["AzureServiceBus:ConnectionString"];
                
                await using var client = new ServiceBusClient(connectionString);
                ServiceBusSender? sender = client.CreateSender("user-history");

                var message = new ServiceBusMessage(JsonSerializer.Serialize(history));
                await sender.SendMessageAsync(message);
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Could not create a request to Service Bus.");
        }
    }
}