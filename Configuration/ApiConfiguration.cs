namespace StudentPeformanceTracker.Configuration;

public class ApiConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;

    public static ApiConfiguration LoadFromEnvironment()
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5199";

        return new ApiConfiguration
        {
            BaseUrl = baseUrl
        };
    }

    public string GetEndpoint(string path)
    {
        // Remove leading slash if present
        if (path.StartsWith("/"))
            path = path.Substring(1);

        // Ensure BaseUrl doesn't end with slash
        var baseUrl = BaseUrl.TrimEnd('/');

        return $"{baseUrl}/{path}";
    }

    // Common API endpoints
    public string LoginEndpoint => GetEndpoint("api/v1/auth/login");
    public string LogoutEndpoint => GetEndpoint("api/v1/auth/logout");
    public string RefreshEndpoint => GetEndpoint("api/v1/auth/refresh");
    public string RegisterStudentEndpoint => GetEndpoint("api/v1/auth/register/student");
    public string RegisterAdminEndpoint => GetEndpoint("api/v1/auth/register/admin");
}
