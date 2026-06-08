// Integration tests — these require the TechMove.GLMS API to be running on http://localhost:5001.
// They are NOT unit tests. Start the API with `dotnet run` (or press F5) before running these.

using System.Net;
using System.Text;
using System.Text.Json;

namespace TechMove.GLMS.Tests.IntegrationTests;

public class ApiIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5001";

    public ApiIntegrationTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    // ── Contracts ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContracts_ReturnsHttpOK()
    {
        var response = await _client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetContracts_ResponseIsNotNull()
    {
        var response = await _client.GetAsync("/api/contracts");
        var body = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    // ── Auth ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var payload = JsonSerializer.Serialize(new { username = "admin", password = "Admin@1234" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var payload = JsonSerializer.Serialize(new { username = "wrong", password = "wrong" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Contracts (auth-gated) ────────────────────────────────────────────────

    [Fact]
    public async Task CreateContract_WithoutAuth_Returns401()
    {
        var payload = JsonSerializer.Serialize(new { title = "Test Contract" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        // No Authorization header — request must be rejected
        var response = await _client.PostAsync("/api/contracts", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Clients ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetClients_ReturnsHttpOK()
    {
        var response = await _client.GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Service Requests ──────────────────────────────────────────────────────

    // ── Data Integrity (Create → Read) ────────────────────────────────────────

    [Fact]
    public async Task CreateClient_ThenReadBack_ReturnsSameData()
    {
        // Step 1: obtain a bearer token
        var loginPayload = JsonSerializer.Serialize(new { username = "admin", password = "Admin@1234" });
        var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        using var loginDoc = JsonDocument.Parse(loginBody);
        var token = loginDoc.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Step 2: create a client
        var uniqueName = $"IntegrationTest-Client-{Guid.NewGuid():N}";
        var createPayload = JsonSerializer.Serialize(new { name = uniqueName, contactDetails = "test@example.com", region = "Gauteng" });
        var createContent = new StringContent(createPayload, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/clients", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var createdId = createDoc.RootElement.GetProperty("id").GetInt32();
        Assert.True(createdId > 0);

        // Step 3: read back the specific client and verify data integrity
        _client.DefaultRequestHeaders.Authorization = null;
        var getResponse = await _client.GetAsync($"/api/clients/{createdId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        using var getDoc = JsonDocument.Parse(getBody);
        var returnedName = getDoc.RootElement.GetProperty("name").GetString();

        Assert.Equal(uniqueName, returnedName);
    }

    [Fact(Skip = "Requires seeded data: a contract in Expired status must exist in the database")]
    public async Task CreateServiceRequest_AgainstExpiredContract_Returns400()
    {
        // Expected behaviour: the API must return 400 Bad Request when a service
        // request is submitted against a contract whose status is Expired, because
        // expired contracts cannot accept new service requests (see WorkflowValidationService).
        //
        // To run this test manually:
        //   1. Seed (or manually set) a contract to ContractStatus.Expired in the DB.
        //   2. Obtain a valid bearer token via POST /api/auth/login.
        //   3. Replace <CONTRACT_ID> and <TOKEN> below, then remove the Skip attribute.

        const string expiredContractId = "<CONTRACT_ID>";
        const string bearerToken = "<TOKEN>";

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var payload = JsonSerializer.Serialize(new
        {
            contractId = expiredContractId,
            description = "Service request against expired contract"
        });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/servicerequests", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose() => _client.Dispose();
}
