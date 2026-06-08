using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechMove.GLMS.Models;

namespace TechMove.GLMS.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly ApiTokenStore _tokenStore;
    private readonly IConfiguration _config;
    private readonly ILogger<ApiService> _logger;
    private static readonly SemaphoreSlim _loginLock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiService(HttpClient http, ApiTokenStore tokenStore, IConfiguration config, ILogger<ApiService> logger)
    {
        _http = http;
        _tokenStore = tokenStore;
        _config = config;
        _logger = logger;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_tokenStore.Token is not null)
            return;

        await _loginLock.WaitAsync();
        try
        {
            if (_tokenStore.Token is not null)
                return;

            var credentials = new
            {
                username = _config["ApiCredentials:Username"],
                password = _config["ApiCredentials:Password"]
            };

            var content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await JsonSerializer.DeserializeAsync<JsonElement>(
                    await response.Content.ReadAsStreamAsync());
                _tokenStore.Token = json.GetProperty("token").GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with the API");
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private async Task<HttpRequestMessage> BuildRequestAsync(HttpMethod method, string url)
    {
        await EnsureAuthenticatedAsync();
        var request = new HttpRequestMessage(method, url);
        if (_tokenStore.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        return request;
    }

    private static StringContent JsonBody<T>(T value) =>
        new(JsonSerializer.Serialize(value, _jsonOptions), Encoding.UTF8, "application/json");

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) return default;
        return await JsonSerializer.DeserializeAsync<T>(
            await response.Content.ReadAsStreamAsync(), _jsonOptions);
    }

    // ── Clients ────────────────────────────────────────────────────────────────

    public async Task<List<Client>> GetClientsAsync()
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Get, "/api/clients");
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<List<Client>>(res) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch clients from API");
            return [];
        }
    }

    public async Task<Client?> GetClientAsync(int id)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Get, $"/api/clients/{id}");
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<Client>(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch client {Id} from API", id);
            return null;
        }
    }

    public async Task<Client?> CreateClientAsync(Client client)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Post, "/api/clients");
            req.Content = JsonBody(client);
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<Client>(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create client via API");
            return null;
        }
    }

    public async Task<bool> UpdateClientAsync(int id, Client client)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Put, $"/api/clients/{id}");
            req.Content = JsonBody(client);
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update client {Id} via API", id);
            return false;
        }
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Delete, $"/api/clients/{id}");
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete client {Id} via API", id);
            return false;
        }
    }

    // ── Contracts ──────────────────────────────────────────────────────────────

    public async Task<List<Contract>> GetContractsAsync(string? status = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = new List<string>();
            if (status is not null) query.Add($"status={Uri.EscapeDataString(status)}");
            if (startDate is not null) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate is not null) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var url = "/api/contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var req = await BuildRequestAsync(HttpMethod.Get, url);
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<List<Contract>>(res) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch contracts from API");
            return [];
        }
    }

    public async Task<Contract?> GetContractAsync(int id)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Get, $"/api/contracts/{id}");
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<Contract>(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch contract {Id} from API", id);
            return null;
        }
    }

    public async Task<Contract?> CreateContractAsync(Contract contract)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Post, "/api/contracts");
            req.Content = JsonBody(contract);
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<Contract>(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contract via API");
            return null;
        }
    }

    public async Task<bool> UpdateContractAsync(int id, Contract contract)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Put, $"/api/contracts/{id}");
            req.Content = JsonBody(contract);
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update contract {Id} via API", id);
            return false;
        }
    }

    public async Task<bool> UpdateContractStatusAsync(int id, ContractStatus status)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Patch, $"/api/contracts/{id}/status");
            req.Content = JsonBody(new { status });
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update contract {Id} status via API", id);
            return false;
        }
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Delete, $"/api/contracts/{id}");
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete contract {Id} via API", id);
            return false;
        }
    }

    // ── Service Requests ───────────────────────────────────────────────────────

    public async Task<List<ServiceRequest>> GetServiceRequestsAsync(int? contractId = null)
    {
        try
        {
            var url = contractId.HasValue
                ? $"/api/servicerequests?contractId={contractId}"
                : "/api/servicerequests";
            var req = await BuildRequestAsync(HttpMethod.Get, url);
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<List<ServiceRequest>>(res) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch service requests from API");
            return [];
        }
    }

    public async Task<ServiceRequest?> CreateServiceRequestAsync(ServiceRequest sr)
    {
        try
        {
            var req = await BuildRequestAsync(HttpMethod.Post, "/api/servicerequests");
            req.Content = JsonBody(sr);
            var res = await _http.SendAsync(req);
            return await DeserializeAsync<ServiceRequest>(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service request via API");
            return null;
        }
    }
}
