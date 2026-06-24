---
name: integration-testing
description: Integration and e2e tests for .NET — WebApplicationFactory, Testcontainers, EF Core in-memory vs real DB. Use when testing endpoints and persistence.
---

# Integration Testing

> **Related skills:** `testing/unit-testing`

## When
Test real component interactions: endpoints, persistence, external integrations — where unit tests with mocks give too little confidence.

## ASP.NET Core endpoints — WebApplicationFactory
```csharp
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public OrdersApiTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Get_UnknownOrder_Returns404()
    {
        var resp = await _client.GetAsync("/orders/999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

## Persistence — real DB via Testcontainers
Use Testcontainers (e.g. SQL Server/PostgreSQL) for realistic DB tests instead of EF in-memory when query behavior matters. In-memory is fine for fast, behavior-independent tests.

## Rules
- Each test cleans up its own data or runs in a transaction/fresh container.
- No dependency on test order.
- External network calls: stub or containerize; no real third-party endpoints in CI.
- Mark slow tests with a trait/category so CI can run them separately.
