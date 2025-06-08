using AIHelperLibrary.Models;
using Microsoft.AspNetCore.Components;

namespace AIHelperDemo.Components.Pages;

public partial class AiDemo : ComponentBase
{
    private AIProvider _selectedProvider = AIProvider.OpenAI;
    private string _prompt = "";
    private string _response = "";
    private string _errorMessage = "";
    private bool _isLoading;

    // UI-only state (not persisted)
    private bool _showOpenAiKey;
    private bool _showAnthropicKey;
    private bool _showAdvanced;
    private int _maxTokens = 500;
    private double _temperature = 0.7;
    private double _topP = 1.0;
    private int _maxRetries = 3;

    // Model selection
    private OpenAIModel _selectedOpenAIModel = OpenAIModel.GPT_3_5_Turbo;
    private AnthropicModel _selectedAnthropicModel = AnthropicModel.Claude3Haiku;

    // Session service methods
    private bool GetUseOwnApiKeys() => SessionService.GetUseOwnApiKeys();
    private void SetUseOwnApiKeys(bool value) => SessionService.SetUseOwnApiKeys(value);

    private string GetOpenAiApiKey() => SessionService.GetUserApiKey("openai");
    private void SetOpenAiApiKey(string value) => SessionService.SetUserApiKey("openai", value);

    private string GetAnthropicApiKey() => SessionService.GetUserApiKey("anthropic");
    private void SetAnthropicApiKey(string value) => SessionService.SetUserApiKey("anthropic", value);

    private int GetDemoRequestsRemaining() => SessionService.GetDemoRequestsRemaining();

    private async Task GenerateResponse()
    {
        if (string.IsNullOrWhiteSpace(_prompt))
            return;

        if (!GetUseOwnApiKeys() && GetDemoRequestsRemaining() <= 0)
        {
            _errorMessage = "Demo limit reached. Please use your own API keys for unlimited access.";
            return;
        }

        _isLoading = true;
        _errorMessage = "";
        _response = "";
        StateHasChanged();

        try
        {
            var apiKey = GetCurrentApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _errorMessage = $"Please provide a valid {_selectedProvider} API key.";
                return;
            }

            var config = GetUseOwnApiKeys() ? GetCurrentConfiguration() : null;
            _response = await DemoService.GenerateTextAsync(_prompt, _selectedProvider,
                GetUseOwnApiKeys() ? apiKey : null, config);

            if (!GetUseOwnApiKeys())
            {
                SessionService.DecrementDemoRequests();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private string GetCurrentApiKey()
    {
        if (!GetUseOwnApiKeys()) return "demo-key";

        return _selectedProvider switch
        {
            AIProvider.OpenAI => GetOpenAiApiKey(),
            AIProvider.Anthropic => GetAnthropicApiKey(),
            _ => ""
        };
    }

    private void SetApiKeyMode(bool useOwnKeys)
    {
        SetUseOwnApiKeys(useOwnKeys);
        _errorMessage = "";
        _response = "";
        StateHasChanged();
    }

    private void SetPrompt(string prompt)
    {
        _prompt = prompt;
        StateHasChanged();
    }

    private void SetQuantumPrompt() => SetPrompt("Explain quantum computing in simple terms");
    private void SetHaikuPrompt() => SetPrompt("Write a haiku about programming");
    private void SetCodePrompt() => SetPrompt("Create a simple Python function to calculate fibonacci numbers");

    private string GetModelDescription(object model)
    {
        return model switch
        {
            OpenAIModel.GPT_3_5_Turbo => "Fast and cost-effective for most tasks",
            OpenAIModel.GPT_4 => "Most capable model with excellent reasoning",
            OpenAIModel.GPT_4_Turbo => "Faster GPT-4 with good performance balance",
            OpenAIModel.GPT_4o => "Optimized for speed and efficiency",
            OpenAIModel.GPT_4o_Mini => "Lightweight version of GPT-4o",
            OpenAIModel.O1_Mini => "Specialized for complex reasoning tasks",
            OpenAIModel.O3_Mini => "Latest reasoning model with improved capabilities",

            AnthropicModel.Claude3Haiku => "Fastest Claude model for quick responses",
            AnthropicModel.Claude3Sonnet => "Balanced performance and capability",
            AnthropicModel.Claude3Opus => "Most powerful Claude model",
            AnthropicModel.Claude3_5_Sonnet => "Enhanced version with better performance",
            AnthropicModel.Claude3_7_Sonnet => "Newest model with latest improvements",

            _ => "Advanced AI model"
        };
    }

    private object? GetCurrentConfiguration()
    {
        if (!GetUseOwnApiKeys()) return null;

        return _selectedProvider switch
        {
            AIProvider.OpenAI => new
            {
                Model = _selectedOpenAIModel,
                MaxTokens = _maxTokens,
                Temperature = _temperature,
                TopP = _topP,
                MaxRetries = _maxRetries
            },
            AIProvider.Anthropic => new
            {
                Model = _selectedAnthropicModel,
                MaxTokens = _maxTokens,
                Temperature = _temperature,
                TopP = _topP,
                MaxRetries = _maxRetries
            },
            _ => null
        };
    }

    private string GetCurrentModelName()
    {
        if (_selectedProvider == AIProvider.OpenAI)
        {
            return GetUseOwnApiKeys() ? _selectedOpenAIModel.ToString().Replace("_", ".") : "GPT-3.5 Turbo";
        }
        else
        {
            return GetUseOwnApiKeys() ? _selectedAnthropicModel.ToString().Replace("_", " ") : "Claude 3 Haiku";
        }
    }

    private string GetProviderButtonClass(AIProvider provider)
    {
        const string baseClass = "p-3 rounded-lg text-sm font-medium border";
        var isDisabled = GetUseOwnApiKeys() &&
                         ((provider == AIProvider.OpenAI && string.IsNullOrWhiteSpace(GetOpenAiApiKey())) ||
                          (provider == AIProvider.Anthropic && string.IsNullOrWhiteSpace(GetAnthropicApiKey())));

        if (isDisabled)
        {
            return $"{baseClass} bg-gray-500/20 border-gray-500/40 text-gray-400 cursor-not-allowed";
        }

        return _selectedProvider == provider
            ? $"{baseClass} bg-white/20 border-white/40 text-white"
            : $"{baseClass} bg-white/5 border-white/20 text-white/70 hover:bg-white/10 hover:text-white";
    }

    private string GetModeButtonClass(bool isOwnKeyMode)
    {
        const string baseClass = "px-4 py-2 rounded-lg text-sm font-medium flex items-center";
        return GetUseOwnApiKeys() == isOwnKeyMode
            ? $"{baseClass} bg-white text-blue-600"
            : $"{baseClass} text-white/70 hover:text-white hover:bg-white/10";
    }

    private bool GetGenerateButtonDisabled()
    {
        if (_isLoading || string.IsNullOrWhiteSpace(_prompt))
            return true;

        if (!GetUseOwnApiKeys() && GetDemoRequestsRemaining() <= 0)
            return true;

        if (GetUseOwnApiKeys())
        {
            var apiKey = GetCurrentApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                return true;
        }

        return false;
    }
}