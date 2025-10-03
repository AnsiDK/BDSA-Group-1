using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplicationFactory;

namespace Chirp.LocalServer;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IMyService, MockMyService>();
        });
        return base.CreateHost(builder);
    }
    public IHost GetHost() => _host ?? throw new InvalidOperationException("Host has not been created yet.");

}