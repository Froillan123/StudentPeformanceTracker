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

        var configOptions = ParseUpstashRedisUrl(redisUrl);

        // Set connection timeout and retry settings for better reliability
        configOptions.ConnectTimeout = 15000; // Increased timeout
        configOptions.SyncTimeout = 10000;
        configOptions.AbortOnConnectFail = false;
        configOptions.ConnectRetry = 3;

        _redis = ConnectionMultiplexer.Connect(configOptions);
        _db = _redis.GetDatabase();
    }

    private ConfigurationOptions ParseUpstashRedisUrl(string redisUrl)
    {
        // Parse Upstash Redis URL format: rediss://default:password@host:port
        var uri = new Uri(redisUrl);
        var password = uri.UserInfo.Split(':')[1]; // Extract password after "default:"

        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{uri.Host}:{uri.Port}" },
            Password = password,
            Ssl = true, // Enable SSL/TLS for Upstash
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
            AbortOnConnectFail = false
        };

        return configOptions;
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

            var result = await transaction.ExecuteAsync();

            if (result)
            {
                Console.WriteLine($"✅ Redis: Successfully stored refresh token for user {userId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Redis Error (StoreRefreshToken): {ex.Message}");
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
            Console.WriteLine($"❌ Redis Error (GetRefreshToken): {ex.Message}");
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
            Console.WriteLine($"❌ Redis Error (RevokeRefreshToken): {ex.Message}");
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
            Console.WriteLine($"❌ Redis Error (GetUserIdFromRefreshToken): {ex.Message}");
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
            Console.WriteLine($"❌ Redis Error (ValidateRefreshToken): {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var result = await _db.PingAsync();
            var isHealthy = result != TimeSpan.Zero;

            if (isHealthy)
            {
                Console.WriteLine($"✅ Redis: Connection healthy (ping: {result.TotalMilliseconds}ms)");
            }
            else
            {
                Console.WriteLine("❌ Redis: Connection unhealthy");
            }

            return isHealthy;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Redis Error (Health Check): {ex.Message}");
            return false;
        }
    }
}
