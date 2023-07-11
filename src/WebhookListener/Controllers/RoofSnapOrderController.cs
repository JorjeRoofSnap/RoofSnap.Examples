using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using WebhookListener.Models;

namespace WebhookListener.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoofSnapOrderController : ControllerBase
{
    private static RoofSnapAuthToken _authToken = new();
    private readonly RoofSnapClientOptions _clientOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RoofSnapOrderController> _logger;

    public RoofSnapOrderController(ILogger<RoofSnapOrderController> logger, IHttpClientFactory httpClientFactory,
        IOptions<RoofSnapClientOptions> clientOptions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _clientOptions = clientOptions.Value;
    }

    /// <summary>
    ///     This is an example of a webhook endpoint that will respond to the order completed event
    /// </summary>
    /// <param name="orderEvent">
    ///     <see cref="RoofSnapOrderEvent" />
    /// </param>
    /// <returns></returns>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> PostAsync(RoofSnapOrderEvent orderEvent)
    {
        _logger.LogInformation("Received OrderEvent:\r\n{orderEvent}",
            JsonSerializer.Serialize(orderEvent,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

        if (orderEvent.EventType != RoofSnapEventType.SketchOrderCompleted || orderEvent.Data == null)
            return BadRequest();

        try
        {
            // Get measurement order using Subject from webhook event
            var order = await GetResourceAsync(orderEvent.Subject);
            
            // Get document using SketchReport id from previous response
            var document = await GetResourceAsync($"/projectdocuments/{order?["SketchReport"]?["Id"]}", "v1");

            // TODO: The order object is a bit on the large side so a full model is not provided in this example
            // this log statement will show you the full properties available on that object.
            _logger.LogInformation("Fetched order:\r\n{order}",
                order?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            _logger.LogInformation("Order document id: '{ProjectDocumentId}'", order?["SketchReport"]?["Id"]);
            
            // Another http client could be used to fetch the pdf from this url
            _logger.LogInformation("Order document url: '{url}'", document?["DocumentUrl"]);
        }
        catch (FailedRoofSnapRequestException e)
        {
            _logger.LogError(e,
                "An error occured attempting to fetch order {OrderId}.\r\nRequested url: {Url},\r\nStatus: {HttpStatusCode},\r\nResponse was: '{ResponseMessage}'",
                orderEvent.Data.ResourceId, e.RequestUrl, e.ResponseStatusCode, e.ResponseMessage);
            return StatusCode(500);
        }

        return NoContent();
    }

    private async Task<JsonNode?> GetResourceAsync(string subject, string version = "v2")
    {
        var token = await GetTokenAsync();

        var request =
            new HttpRequestMessage(HttpMethod.Get, $"https://roofsnap.azure-api.net/dev/{version}{subject}")
            {
                Headers =
                {
                    { "Ocp-Apim-Subscription-Key", _clientOptions.SubscriptionKey },
                    { HeaderNames.Authorization, $"Bearer {token.AccessToken}" }
                }
            };

        var response = await SendRequestAsync(request);
        
        await using var stream = await response.Content.ReadAsStreamAsync();
        return JsonNode.Parse(stream);
    }

    private async Task<RoofSnapAuthToken> GetTokenAsync()
    {
        if (_authToken.IsValid) return _authToken;

        var formDict = new Dictionary<string, string>
        {
            { "username", _clientOptions.Username },
            { "password", _clientOptions.Password },
            { "grant_type", "password" },
            { "client_id", _clientOptions.ClientId }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://dev-auth2.roofsnap.com/oauth2/token")
        {
            Content = new FormUrlEncodedContent(formDict)
        };

        var response = await SendRequestAsync(httpRequestMessage);

        await using var stream = await response.Content.ReadAsStreamAsync();

        _authToken = await JsonSerializer.DeserializeAsync<RoofSnapAuthToken>(stream) ?? new RoofSnapAuthToken();

        return _authToken;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage message)
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.SendAsync(message);

        if (!response.IsSuccessStatusCode) throw new FailedRoofSnapRequestException(message, response);

        return response;
    }
}