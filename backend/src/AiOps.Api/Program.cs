using AiOps.Api.Endpoints;
using AiOps.Application;
using AiOps.Infrastructure;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapHealthEndpoints();

app.Run();

public partial class Program;
