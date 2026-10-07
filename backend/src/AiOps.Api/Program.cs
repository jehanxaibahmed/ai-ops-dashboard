using System.Text.Json.Serialization;
using AiOps.Api.Endpoints;
using AiOps.Api.Infrastructure;
using AiOps.Api.Realtime;
using AiOps.Application;
using AiOps.Application.Abstractions;
using AiOps.Infrastructure;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IJobNotifier, SignalRJobNotifier>();
builder.Services.AddSingleton<ISimulationNotifier, SignalRSimulationNotifier>();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173"];

    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AiOps.Infrastructure.Persistence.AiOpsDbContext>();
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(db.Database);
            break;
        }
        catch (Exception) when (attempt < 10)
        {
            Thread.Sleep(2000);
        }
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapHealthEndpoints();
app.MapCatalogEndpoints();
app.MapJobEndpoints();
app.MapFailureEndpoints();
app.MapAnalyticsEndpoints();
app.MapSimulationEndpoints();
app.MapOpenAiProxyEndpoints();
app.MapHub<JobsHub>(JobsHub.Route);

app.Run();

public partial class Program;
