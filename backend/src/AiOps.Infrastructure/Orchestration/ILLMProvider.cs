using Microsoft.Extensions.AI;

namespace AiOps.Infrastructure.Orchestration;

public interface ILLMProvider
{
    IChatClient GetClient(string modelId);
}
