using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JwtBearerExample.API.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public ClientController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClient = httpClientFactory.CreateClient("Server");
        _memoryCache = memoryCache;
    }

    [HttpGet(Name = "ClientJwt")]
    public async Task<IActionResult> ClientJwt([FromQuery] bool simulateBadRequest)
    {
        var token = _memoryCache.Get<string>("Token");

        if (token == default)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            var responseAuthentication = await _httpClient.GetAsync("/Authentication");

            if (responseAuthentication.IsSuccessStatusCode)
            {
                var tokenResponse = await responseAuthentication.Content.ReadFromJsonAsync<Authentication>();

                _memoryCache.Set("Token", tokenResponse?.AccessToken);

                token = tokenResponse?.AccessToken;
            }
            else
            {
                return BadRequest("Error on Authentication");
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.GetAsync($"/Authentication/NeedToken?simulatebadrequest={simulateBadRequest}");

        var headers = response.Headers;

        var content = await response.Content.ReadAsStringAsync();

        var result = new
        {
            Headers = headers,
            Content = content
        };

        if (response.IsSuccessStatusCode)
        {
            return Ok(result);
        }

        return StatusCode((int) response.StatusCode, result);
        
    }
}