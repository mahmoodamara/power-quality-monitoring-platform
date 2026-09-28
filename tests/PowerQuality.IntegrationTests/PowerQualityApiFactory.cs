using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.IntegrationTests;
public sealed class PowerQualityApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PowerQualityDbContext>>();
            services.RemoveAll<PowerQualityDbContext>();
            services.AddDbContext<PowerQualityDbContext>(o => o.UseInMemoryDatabase($"PowerQualityTests-{Guid.NewGuid()}"));
            services.RemoveAll<IDeviceStateCache>();
            services.AddSingleton<IDeviceStateCache, FakeDeviceStateCache>();
        });
    }
}
