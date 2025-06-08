using AIHelperDemo.Services.Interfaces;

namespace AIHelperDemo.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string DEMO_REQUESTS_KEY = "DemoRequestsRemaining";
    private const string USE_OWN_KEYS_KEY = "UseOwnApiKeys";
    private const string OPENAI_KEY = "OpenAIApiKey";
    private const string ANTHROPIC_KEY = "AnthropicApiKey";
    private const int DEFAULT_DEMO_REQUESTS = 10;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session
                                ?? throw new InvalidOperationException("Session is not available");

    public int GetDemoRequestsRemaining()
    {
        var remaining = Session.GetInt32(DEMO_REQUESTS_KEY);
        if (!remaining.HasValue)
        {
            // First visit - initialize with default count
            Session.SetInt32(DEMO_REQUESTS_KEY, DEFAULT_DEMO_REQUESTS);
            return DEFAULT_DEMO_REQUESTS;
        }

        return remaining.Value;
    }

    public void DecrementDemoRequests()
    {
        var current = GetDemoRequestsRemaining();
        var newValue = Math.Max(0, current - 1);
        Session.SetInt32(DEMO_REQUESTS_KEY, newValue);
    }

    public void ResetDemoRequests()
    {
        Session.SetInt32(DEMO_REQUESTS_KEY, DEFAULT_DEMO_REQUESTS);
    }

    public bool GetUseOwnApiKeys()
    {
        return Session.GetString(USE_OWN_KEYS_KEY) == "true";
    }

    public void SetUseOwnApiKeys(bool value)
    {
        Session.SetString(USE_OWN_KEYS_KEY, value ? "true" : "false");
    }

    public string GetUserApiKey(string provider)
    {
        var key = provider.ToLower() switch
        {
            "openai" => OPENAI_KEY,
            "anthropic" => ANTHROPIC_KEY,
            _ => throw new ArgumentException($"Unknown provider: {provider}")
        };

        return Session.GetString(key) ?? "";
    }

    public void SetUserApiKey(string provider, string apiKey)
    {
        var key = provider.ToLower() switch
        {
            "openai" => OPENAI_KEY,
            "anthropic" => ANTHROPIC_KEY,
            _ => throw new ArgumentException($"Unknown provider: {provider}")
        };

        Session.SetString(key, apiKey ?? "");
    }

    public void ClearSession()
    {
        Session.Clear();
    }
}