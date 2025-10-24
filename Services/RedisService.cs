using StackExchange.Redis;

namespace StudentPeformanceTracker.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService()
    {
        var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL")
            ?? throw new InvalidOperationException("REDIS_URL not found in environment variables");

        var configOptions = ConfigurationOptions.Parse(redisUrl);

        // Set connection timeout and retry settings for better reliability
        configOptions.ConnectTimeout = 10000;
        configOptions.SyncTimeout = 5000;
        configOptions.AbortOnConnectFail = false;

        _redis = ConnectionMultiplexer.Connect(configOptions);
        _db = _redis.GetDatabase();
    }

    public async Task<bool> StoreRefreshTokenAsync(int userId, string token, DateTime expiry)
    {
        try
        {
            var key = $"refresh_token:{userId}";
            var reverseKey = $"refresh_token_lookup:{token}";
            var timeToLive = expiry - DateTime.UtcNow;

            if (timeToLive.TotalSeconds <= 0)
                return false;

            // Store both the token and the reverse lookup
            var transaction = _db.CreateTransaction();
            _ = transaction.StringSetAsync(key, token, timeToLive);
            _ = transaction.StringSetAsync(reverseKey, userId.ToString(), timeToLive);
            
            return await transaction.ExecuteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error (StoreRefreshToken): {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetRefreshTokenAsync(int userId)
    {
        try
        {
            var key = $"refresh_token:{userId}";
            var token = await _db.StringGetAsync(key);
            return token.HasValue ? token.ToString() : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error (GetRefreshToken): {ex.Message}");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(int userId)
    {
        try
        {
            var key = $"refresh_token:{userId}";
            var token = await _db.StringGetAsync(key);
            
            if (token.HasValue)
            {
                var reverseKey = $"refresh_token_lookup:{token}";
                var transaction = _db.CreateTransaction();
                _ = transaction.KeyDeleteAsync(key);
                _ = transaction.KeyDeleteAsync(reverseKey);
                return await transaction.ExecuteAsync();
            }
            
            return true; // Already revoked or doesn't exist
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error (RevokeRefreshToken): {ex.Message}");
            return false;
        }
    }

    public async Task<int?> GetUserIdFromRefreshTokenAsync(string token)
    {
        try
        {
            var reverseKey = $"refresh_token_lookup:{token}";
            var userIdString = await _db.StringGetAsync(reverseKey);
            return userIdString.HasValue && int.TryParse(userIdString, out var userId) ? userId : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error (GetUserIdFromRefreshToken): {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(int userId, string token)
    {
        try
        {
            var storedToken = await GetRefreshTokenAsync(userId);
            return storedToken != null && storedToken == token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error (ValidateRefreshToken): {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            return await _db.PingAsync() != TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }
}
