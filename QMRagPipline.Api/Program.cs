using Microsoft.Extensions.Options;
using OpenAI;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Services;
using QMRagPipeline.Settings;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Load settings configs into configurations
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// set logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// set configurations
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<DataSourceSettings>(builder.Configuration.GetSection("DataSource"));
builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection("Qdrant"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect($"{settings.Host}:{settings.Port}");
});

// register services
builder.Services.AddHttpClient<ISimilaritySearch, QdrantRestSimilaritySearch>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
    client.BaseAddress = new Uri(settings.Url);
    client.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
});

builder.Services.AddScoped<OpenAIClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
    return new OpenAIClient(settings.ApiKey);
});

builder.Services.AddScoped<IDataLoader>(sp =>
{
    var filePath = sp.GetRequiredService<IOptions<DataSourceSettings>>().Value.TextFilePath;
    return new FileDocumentDataLoader(filePath);
});
builder.Services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
//builder.Services.AddScoped<ISimilaritySearch, QdrantRestSimilaritySearch>();
builder.Services.AddScoped<IPromptComposer, DefaultPromptComposer>();
builder.Services.AddScoped<ILlmService, OpenAiLlmService>();
builder.Services.AddScoped<IChatHistoryStore, ChatHistoryStore>();
builder.Services.AddScoped<RagPipelineRunner>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("X-Session-Id")
);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
