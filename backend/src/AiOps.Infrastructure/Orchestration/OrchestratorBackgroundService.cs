using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using System.Collections.Generic;

namespace AiOps.Infrastructure.Orchestration;

public class OrchestratorBackgroundService : BackgroundService
{
    private readonly IJobRepository _jobs;
    private readonly ILLMProvider _llmProvider;
    private readonly ICatalog _catalog;
    private readonly IJobNotifier _notifier;
    private readonly ILogger<OrchestratorBackgroundService> _logger;
    private readonly TimeProvider _clock;

    public OrchestratorBackgroundService(
        IJobRepository jobs,
        ILLMProvider llmProvider,
        ICatalog catalog,
        IJobNotifier notifier,
        ILogger<OrchestratorBackgroundService> logger,
        TimeProvider clock)
    {
        _jobs = jobs;
        _llmProvider = llmProvider;
        _catalog = catalog;
        _notifier = notifier;
        _logger = logger;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2), _clock);
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var queuedJobs = await _jobs.QueryAsync(new JobFilter { Statuses = [JobStatus.Queued] }, stoppingToken);
                foreach (var job in queuedJobs)
                {
                    await ProcessJobAsync(job, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing jobs.");
            }
        }
    }

    private async Task ProcessJobAsync(Job job, CancellationToken ct)
    {
        var now = _clock.GetUtcNow();
        job.Start(now);
        await _jobs.UpdateAsync(job, ct);
        await _notifier.JobChangedAsync(JobDto.From(job), ct);

        try
        {
            var client = _llmProvider.GetClient(job.Model);
            var messages = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(job.SystemInstructions))
            {
                messages.Add(new ChatMessage(ChatRole.System, job.SystemInstructions));
            }
            messages.Add(new ChatMessage(ChatRole.User, job.Prompt));

            var response = await client.GetResponseAsync(messages, cancellationToken: ct);
            
            var inputTokens = response.Usage?.InputTokenCount ?? 0;
            var outputTokens = response.Usage?.OutputTokenCount ?? 0;
            var cost = _catalog.FindModel(job.Model)?.Pricing.CostOf(inputTokens, outputTokens) ?? 0m;

            now = _clock.GetUtcNow();
            job.ReportProgress(99, inputTokens, outputTokens, cost, now);
            job.Succeed(response.Text ?? "", now);
            
            await _jobs.UpdateAsync(job, ct);
            await _notifier.JobChangedAsync(JobDto.From(job), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed.", job.Id);
            now = _clock.GetUtcNow();
            job.Fail(new JobFailure("LLM_ERROR", ex.Message, true), now);
            await _jobs.UpdateAsync(job, ct);
            await _notifier.JobChangedAsync(JobDto.From(job), ct);
        }
    }
}
