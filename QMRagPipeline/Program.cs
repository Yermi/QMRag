using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Services;
using QMRagPipeline.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


//builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<DataSourceSettings>(builder.Configuration.GetSection("DataSource"));

builder.Services.AddSingleton<OpenAIClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
    return new OpenAIClient(settings.ApiKey);
});

// Register services (DI)
builder.Services.AddScoped<IDataLoader>(sp =>
{
    var filePath = sp.GetRequiredService<IOptions<DataSourceSettings>>().Value.TextFilePath;
    return new FileDocumentDataLoader(filePath);
});
builder.Services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
builder.Services.AddScoped<ISimilaritySearch, InMemorySimilaritySearch>();
builder.Services.AddSingleton<IPromptComposer, DefaultPromptComposer>();
builder.Services.AddSingleton<ILlmService, OpenAiLlmService>();
builder.Services.AddSingleton<RagPipelineRunner>();

var host = builder.Build();

var pipeline = host.Services.GetRequiredService<RagPipelineRunner>();
await pipeline.BuildIndexAsync();

Console.WriteLine("[READY] Type your question:");
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;

    var answer = await pipeline.AnswerQuestionAsync(input);
    Console.WriteLine("\n[GPT-4o]\n" + answer);
    Console.WriteLine("\n[Ask another question or Ctrl+C to exit]\n");
}