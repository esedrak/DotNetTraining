using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Basics.Tests;

/// <summary>
/// Demonstrates in-process HTTP testing with <c>WebApplication</c> +
///
/// <c>UseTestServer()</c> replaces the real Kestrel socket with an in-process
/// transport, so the full middleware pipeline runs with no network overhead.
/// </summary>
public class HttpTestTests : IAsyncLifetime
{
    // ── Shared in-process test app ────────────────────────────────────────────

    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();       // ← swap Kestrel for in-process

        _app = builder.Build();

        _app.MapGet("/hello", () => Results.Text("Hello, World!"));

        _app.MapPost("/echo", async (HttpRequest req) =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            return Results.Text(body, "application/json", statusCode: 201);
        });

        _app.MapGet("/header", (HttpRequest req) =>
        {
            var val = req.Headers["X-Custom-Header"].FirstOrDefault() ?? "";
            return Results.Text(val);
        });

        await _app.StartAsync();
        _client = _app.GetTestClient();        // ← from Microsoft.AspNetCore.TestHost
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    // ── GET ───────────────────────────────────────────────────────────────────

    /// <summary>
    ///  no real network.
    /// </summary>
    [Fact]
    public async Task Get_HelloEndpoint_Returns200WithBody()
    {
        var response = await _client.GetAsync("/hello");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Hello, World!");
    }

    // ── POST with JSON body ───────────────────────────────────────────────────

    /// <summary>
    /// POST a JSON payload and inspect
    /// the response body and status code.
    /// </summary>
    [Fact]
    public async Task Post_EchoEndpoint_ReturnsCreatedWithEchoedBody()
    {
        var payload = JsonSerializer.Serialize(new { message = "hello" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/echo", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("hello");
    }

    // ── Non-2xx status codes ──────────────────────────────────────────────────

    /// <summary>
    /// 4xx/5xx are NOT exceptions
    /// in .NET; callers must inspect <c>response.StatusCode</c>.
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task NonSuccessStatus_IsNotAnException(int statusCode)
    {
        var b = WebApplication.CreateBuilder();
        b.WebHost.UseTestServer();
        var testApp = b.Build();
        testApp.Map("/", () => Results.StatusCode(statusCode));
        await testApp.StartAsync();

        using var client = testApp.GetTestClient();

        // Does NOT throw — check StatusCode explicitly
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be((HttpStatusCode)statusCode);

        await testApp.StopAsync();
        await testApp.DisposeAsync();
    }

    // ── Custom request headers ────────────────────────────────────────────────

    /// <summary>
    /// custom headers
    /// on the outgoing request arrive at the handler unchanged.
    /// </summary>
    [Fact]
    public async Task RequestHeaders_ArePropagatedToHandler()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/header");
        request.Headers.Add("X-Custom-Header", "my-value");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("my-value");
    }
}
