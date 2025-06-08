namespace AIHelperDemo.Services.Interfaces;

public interface ISessionService
{
    int GetDemoRequestsRemaining();
    void DecrementDemoRequests();
    void ResetDemoRequests();
    bool GetUseOwnApiKeys();
    void SetUseOwnApiKeys(bool value);
    string GetUserApiKey(string provider);
    void SetUserApiKey(string provider, string apiKey);
    void ClearSession();
}