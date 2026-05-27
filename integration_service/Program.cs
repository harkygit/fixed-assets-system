using System.Reflection;
using FixedAssets.Integration;
using FixedAssets.Integration.Adapters;
using FixedAssets.Integration.DTO;
using FixedAssets.Integration.Logging;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;

LoggerConfig.ConfigureLogger();

try
{
    Log.Information("Starting Fixed Assets IntegrationService");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "Fixed Assets Integration API",
                Version = "v1",
                Description = "Integration layer API for fixed asset lifecycle workflows."
            });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    var redisConnection = builder.Configuration.GetConnectionString("Redis")
        ?? builder.Configuration["Redis:Configuration"]
        ?? "redis:6379";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "fixed-assets:";
    });

    builder.Services.AddHttpClient<ObjectAdapter>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:ObjectService"]
            ?? "http://object-service:5001");
    });

    builder.Services.AddHttpClient<DepreciationAdapter>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:DepreciationService"]
            ?? "http://depreciation-service:5002");
    });

    builder.Services.AddHttpClient<DisposalAdapter>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:DisposalService"]
            ?? "http://disposal-service:3000");
    });

    builder.Services.AddScoped<IntegrationService>();
    builder.Services.AddScoped<BusinessProcessService>();
    builder.Services.AddScoped<StockService>();
    builder.Services.AddScoped<SagaOrchestrator>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixed Assets Integration API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "Healthy",
        service = "IntegrationService",
        checkedAt = DateTimeOffset.UtcNow
    }))
    .WithName("HealthCheck")
    .WithSummary("Returns IntegrationService health status.")
    .WithOpenApi();

    app.MapGet("/api/assets", async (IntegrationService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.GetAssetsAsync(cancellationToken)))
    .WithName("GetAssets")
    .WithSummary("Returns fixed assets through the integration layer with Redis caching.")
    .Produces<IReadOnlyCollection<ObjectDto>>(StatusCodes.Status200OK)
    .WithOpenApi();

    app.MapPost("/api/integration/fixed-assets/dispose", async (
        FixedAssetWorkflowRequest request,
        IntegrationService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ExecuteFixedAssetWorkflowAsync(request, cancellationToken)))
    .WithName("DisposeFixedAssetWorkflow")
    .WithSummary("Runs the frontend-to-integration demo workflow for fixed asset disposal.")
    .Accepts<FixedAssetWorkflowRequest>("application/json")
    .Produces<FixedAssetWorkflowResponse>(StatusCodes.Status200OK)
    .ProducesValidationProblem()
    .WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "IntegrationService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
