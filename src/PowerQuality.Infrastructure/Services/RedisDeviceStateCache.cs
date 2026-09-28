using System.Text.Json;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
using StackExchange.Redis;
namespace PowerQuality.Infrastructure.Services;
public sealed class RedisDeviceStateCache(IConnectionMultiplexer redis, ILogger<RedisDeviceStateCache> logger) : IDeviceStateCache
{
    private static string Key(Guid id) => $"device:{id}:live";
    public async Task SetAsync(DeviceLiveState state, CancellationToken ct)
    {
        try { await redis.GetDatabase().StringSetAsync(Key(state.DeviceId), JsonSerializer.Serialize(state), TimeSpan.FromHours(1)); }
        catch (Exception ex) { logger.LogWarning(ex, "Redis cache write failed for device {DeviceId}", state.DeviceId); }
    }
    public async Task<DeviceLiveState?> GetAsync(Guid deviceId, CancellationToken ct)
    {
        try
        {
            var value = await redis.GetDatabase().StringGetAsync(Key(deviceId));
            return value.HasValue ? JsonSerializer.Deserialize<DeviceLiveState>(value.ToString()) : null;
        }
        catch (Exception ex) { logger.LogWarning(ex, "Redis cache read failed for device {DeviceId}", deviceId); return null; }
    }
}
