using Microsoft.Extensions.Options;
using OpenAI;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Services;
using QMRagPipeline.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<DataSourceSettings>(builder.Configuration.GetSection("DataSource"));
builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection("Qdrant"));
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
builder.Services.AddScoped<RagPipelineRunner>();

// Add services to the container.

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

//var pipeline = app.Services.GetRequiredService<RagPipelineRunner>();
//await pipeline.BuildIndexAsync();

using (var scope = app.Services.CreateScope())
{
    var pipeline = scope.ServiceProvider.GetRequiredService<RagPipelineRunner>();
    await pipeline.BuildIndexAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
