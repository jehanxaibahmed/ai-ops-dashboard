using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;

namespace AiOps.Infrastructure.Orchestration;

public class LLMProvider : ILLMProvider
{
    private readonly IConfiguration _config;
    
    public LLMProvider(IConfiguration config)
    {
        _config = config;
    }

    public IChatClient GetClient(string modelId)
    {
        if (modelId.StartsWith("gpt-"))
        {
            var apiKey = _config["AI:OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("OpenAI API key missing");
            return new ChatClient(modelId, apiKey).AsIChatClient();
        }
        
        var endpoint = _config["AI:Ollama:Endpoint"] ?? "http://localhost:11434";
        return new OllamaChatClient(new Uri(endpoint), modelId);
    }
}
