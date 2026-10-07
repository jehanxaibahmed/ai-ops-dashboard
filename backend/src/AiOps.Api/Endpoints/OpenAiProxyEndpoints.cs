using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using AiOps.Application.Abstractions;
using AiOps.Domain.Jobs;
using AiOps.Infrastructure.Orchestration;

namespace AiOps.Api.Endpoints;

public static class OpenAiProxyEndpoints
{
    public static void MapOpenAiProxyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/chat/completions", HandleChatCompletionAsync)
            .WithTags("OpenAI Proxy")
            .AllowAnonymous();
    }

    private static async Task<IResult> HandleChatCompletionAsync(
        [FromBody] OpenAiChatRequest request,
        [FromServices] IJobRepository jobRepository,
        [FromServices] ILLMProvider llmProvider,
        [FromServices] TimeProvider clock,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Model) || request.Messages == null || request.Messages.Count == 0)
        {
            return Results.BadRequest(new { error = new { message = "Invalid request payload." } });
        }

        // Extract prompt and system instructions
        var systemMessage = request.Messages.FirstOrDefault(m => string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase));
        var systemInstructions = systemMessage?.Content ?? string.Empty;

        var lastMessage = request.Messages.LastOrDefault();
        var prompt = lastMessage?.Content ?? string.Empty;

        var now = clock.GetUtcNow();
        var job = new Job(Guid.NewGuid(), "OpenAIProxy", request.Model, "ProxyRequest", systemInstructions, prompt, now);
        
        await jobRepository.AddAsync(job, ct);

        job.Start(clock.GetUtcNow());
        await jobRepository.UpdateAsync(job, ct);

        try
        {
            var actualModel = "qwen2.5-coder:14b";
            var client = llmProvider.GetClient(actualModel);
            var aiMessages = request.Messages.Select(m => 
                new ChatMessage(new ChatRole(m.Role), m.Content)
            ).ToList();

            // We append a reminder to the system prompt to ensure JSON output
            var systemMsg = aiMessages.FirstOrDefault(m => m.Role == ChatRole.System);
            
            var schemaText = "";
            if (request.ResponseFormat != null)
            {
                schemaText = $"\n\nCRITICAL: Output ONLY valid JSON matching the exact schema requested. Do not include markdown formatting or conversational text.\nSchema:\n{request.ResponseFormat.ToJsonString()}";
            }
            else
            {
                schemaText = "\n\nCRITICAL: Output ONLY valid JSON matching the exact schema requested. Do not include markdown formatting or conversational text.";
            }

            if (systemMsg != null) {
                var newText = systemMsg.Text + schemaText;
                aiMessages.Remove(systemMsg);
                aiMessages.Insert(0, new ChatMessage(ChatRole.System, newText));
            } else {
                aiMessages.Insert(0, new ChatMessage(ChatRole.System, schemaText));
            }

            var options = new ChatOptions { ResponseFormat = ChatResponseFormat.Json };
            var response = await client.GetResponseAsync(aiMessages, options, ct);
            
            var generatedText = response.Text ?? string.Empty;
            
            // Clean up any markdown code fences the model might still emit
            if (generatedText.StartsWith("```json")) {
                generatedText = generatedText.Substring(7);
                if (generatedText.EndsWith("```")) {
                    generatedText = generatedText.Substring(0, generatedText.Length - 3);
                }
                generatedText = generatedText.Trim();
            }

            // Usage might be null if provider doesn't report it
            var inputTokens = response.Usage?.InputTokenCount ?? 0;
            var outputTokens = response.Usage?.OutputTokenCount ?? 0;

            job.ReportProgress(100, inputTokens, outputTokens, 0, clock.GetUtcNow());
            job.Succeed(generatedText, clock.GetUtcNow());
            await jobRepository.UpdateAsync(job, ct);

            // Map result to OpenAI compatible JSON
            return Results.Ok(new
            {
                id = $"chatcmpl-{job.Id}",
                @object = "chat.completion",
                created = job.CreatedAt.ToUnixTimeSeconds(),
                model = actualModel,
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = generatedText
                        },
                        finish_reason = response.FinishReason?.ToString()?.ToLower() ?? "stop"
                    }
                },
                usage = new
                {
                    prompt_tokens = inputTokens,
                    completion_tokens = outputTokens,
                    total_tokens = inputTokens + outputTokens
                }
            });
        }
        catch (Exception ex)
        {
            job.Fail(new JobFailure("ProxyError", ex.Message, true), clock.GetUtcNow());
            await jobRepository.UpdateAsync(job, ct);
            return Results.InternalServerError(new { error = new { message = ex.Message } });
        }
    }
}

public class OpenAiChatRequest
{
    public string Model { get; set; } = string.Empty;
    public List<OpenAiMessage> Messages { get; set; } = new();
    
    [System.Text.Json.Serialization.JsonPropertyName("response_format")]
    public System.Text.Json.Nodes.JsonNode? ResponseFormat { get; set; }
}

public class OpenAiMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
