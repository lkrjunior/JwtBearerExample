using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JwtBearerExample.API.Client.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController : ControllerBase
{
    private const string TokenKey = "Token";
    private const string TokenExpired = "token expired";
    private const string HeaderAuthenticate = "WWW-Authenticate";
    private const string HeaderBearer = "Bearer";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public ClientController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClient = httpClientFactory.CreateClient("Server");
        _memoryCache = memoryCache;
    }

    private string GetValueFromMemoryCache(string key)
    {
        return _memoryCache.Get<string>(key);
    }

    private void RemoveValueFromMemoryCache(string key)
    {
        _memoryCache.Remove(key);
    }

    private void SetValueOnMemoryCache(string key, string value)
    {
        _memoryCache.Set(key, value);
    }

    private void EnsureTokenRefresh(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues(HeaderAuthenticate, out var values) && values.Where(x => x.Contains(TokenExpired)).Count() > 0)
        {
            RemoveValueFromMemoryCache(TokenKey);
        }
    }

    private async Task<string?> GetToken()
    {
        _httpClient.DefaultRequestHeaders.Clear();

        using var responseAuthentication = await _httpClient.GetAsync("/Authentication");

        var tokenResponse = await responseAuthentication.Content.ReadFromJsonAsync<Authentication>();

        if (responseAuthentication.IsSuccessStatusCode && !string.IsNullOrEmpty(tokenResponse?.AccessToken))
        {
            SetValueOnMemoryCache(TokenKey, tokenResponse.AccessToken);

            return tokenResponse.AccessToken;
        }

        return default;
    }

    [HttpGet(Name = "ClientJwt")]
    public async Task<IActionResult> ClientJwt([FromQuery] bool simulateBadRequest, [FromQuery] bool getToken = true)
    {
        var token = default(string);

        if (getToken)
        {
            token = GetValueFromMemoryCache(TokenKey) ?? await GetToken();

            if (token == default)
            {
                return BadRequest("Error to get token on Authentication");
            }
        }
        
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderBearer, token);

            using var response = await _httpClient.GetAsync($"/Authentication/NeedToken?simulatebadrequest={simulateBadRequest}");

            var headers = response.Headers;

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(content);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                EnsureTokenRefresh(headers);
            }

            return StatusCode((int)response.StatusCode, new { Headers = headers, Content = content });

        }
        catch (HttpRequestException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
        }
        
    }
}