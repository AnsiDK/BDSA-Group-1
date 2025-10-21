using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Chirp.LocalServer;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _host = base.CreateHost(builder);
        return _host;
    }

    public IHost GetHost() => _host ?? throw new InvalidOperationException("Host has not been created yet.");
}
