using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace PricingService.IntegrationTest;

public class IntegrationTestsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer pgSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("lab_netmicro_pricing")
        .WithCleanUp(true)
        .Build();
    private WebApplicationFactory<Program>? applicationFactory;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await pgSqlContainer.StartAsync();

        applicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment("IntegrationTest");
                webHostBuilder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = pgSqlContainer.GetConnectionString()
                    });
                });
            });

        Client = applicationFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        if (applicationFactory is not null)
        {
            await applicationFactory.DisposeAsync();
        }

        await pgSqlContainer.DisposeAsync();
    }
}
