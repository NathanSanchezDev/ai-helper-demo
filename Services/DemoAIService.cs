using AIHelperLibrary.Abstractions;
using AIHelperLibrary.Configurations;
using AIHelperLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace AIHelperDemo.Services;

public class DemoAiService
{
    private readonly IAIProviderFactory _aiFactory;
    private readonly IConfiguration _configuration;
    private IAIClient? CurrentClient { get; set; }

    public DemoAiService(IAIProviderFactory aiFactory, IConfiguration configuration)
    {
        _aiFactory = aiFactory;
        _configuration = configuration;
    }

    public async Task<string> GenerateTextAsync(string prompt, AIProvider provider, string? userApiKey = null,
        object? customConfig = null)
    {
        var client = GetOrCreateClient(provider, userApiKey, customConfig);
        return await client.GenerateTextAsync(prompt);
    }

    private IAIClient GetOrCreateClient(AIProvider provider, string? userApiKey = null, object? customConfig = null)
    {
        var apiKey = userApiKey ?? GetDemoApiKey(provider);

        AIBaseConfiguration config = provider switch
        {
            AIProvider.OpenAI => CreateOpenAIConfiguration(customConfig),
            AIProvider.Anthropic => CreateAnthropicConfiguration(customConfig),
            _ => throw new ArgumentException("Unsupported provider")
        };

        CurrentClient = _aiFactory.CreateClient(apiKey, config);
        return CurrentClient;
    }

    private OpenAIConfiguration CreateOpenAIConfiguration(object? customConfig = null)
    {
        if (customConfig != null)
        {
            // Extract custom configuration from anonymous object
            var config = customConfig.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(customConfig));

            return new OpenAIConfiguration
            {
                DefaultModel = config.ContainsKey("Model") ? (OpenAIModel)config["Model"]! : OpenAIModel.GPT_3_5_Turbo,
                MaxTokens = config.ContainsKey("MaxTokens") ? (int)config["MaxTokens"]! : 500,
                Temperature = config.ContainsKey("Temperature") ? (double)config["Temperature"]! : 0.7,
                TopP = config.ContainsKey("TopP") ? (double)config["TopP"]! : 1.0,
                MaxRetryCount = config.ContainsKey("MaxRetries") ? (int)config["MaxRetries"]! : 3,
                EnableLogging = true
            };
        }

        // Default demo configuration
        return new OpenAIConfiguration
        {
            DefaultModel = OpenAIModel.GPT_3_5_Turbo,
            MaxTokens = 150,
            Temperature = 0.7,
            TopP = 1.0,
            MaxRetryCount = 3,
            EnableLogging = true
        };
    }

    private AnthropicConfiguration CreateAnthropicConfiguration(object? customConfig = null)
    {
        if (customConfig != null)
        {
            // Extract custom configuration from anonymous object
            var config = customConfig.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(customConfig));

            return new AnthropicConfiguration
            {
                DefaultModel = config.ContainsKey("Model")
                    ? (AnthropicModel)config["Model"]!
                    : AnthropicModel.Claude3Haiku,
                MaxTokens = config.ContainsKey("MaxTokens") ? (int)config["MaxTokens"]! : 500,
                Temperature = config.ContainsKey("Temperature") ? (double)config["Temperature"]! : 0.7,
                TopP = config.ContainsKey("TopP") ? (double)config["TopP"]! : 1.0,
                MaxRetryCount = config.ContainsKey("MaxRetries") ? (int)config["MaxRetries"]! : 3,
                EnableLogging = true
            };
        }

        // Default demo configuration
        return new AnthropicConfiguration
        {
            DefaultModel = AnthropicModel.Claude3Haiku,
            MaxTokens = 150,
            Temperature = 0.7,
            TopP = 1.0,
            MaxRetryCount = 3,
            EnableLogging = true
        };
    }

    private string GetDemoApiKey(AIProvider provider)
    {
        return provider switch
        {
            AIProvider.OpenAI => _configuration["ApiKeys:OpenAI"]
                                 ?? throw new InvalidOperationException("OpenAI API key not configured"),
            AIProvider.Anthropic => _configuration["ApiKeys:Anthropic"]
                                    ?? throw new InvalidOperationException("Anthropic API key not configured"),
            _ => throw new ArgumentException("No demo key available")
        };
    }
}