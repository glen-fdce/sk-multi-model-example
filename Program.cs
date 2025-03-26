// rough gpt-4o pricing:
// 4,000 context, $0.00008/M input, $0.00032/M output, $0.00008/K input images

// rough claude-3.7-sonnet pricing:
// 200,000 context, $3/M input, $15/M output, $4.8/K input images

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json",
        optional: false);
IConfiguration configuration = builder.Build();
    
// Warning disabling below is because proving a custom OpenAI compatible endpoint
// rather than the default OpenAI endpoint is still experimental.
#pragma warning disable SKEXP0010
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o",
        endpoint: configuration["AzureOpenAI:Endpoint"]!,
        apiKey: configuration["AzureOpenAI:ApiKey"]!,
        serviceId: "azure-gpt-4o")
    .AddOpenAIChatCompletion(
        modelId: "anthropic/claude-3.7-sonnet",
        endpoint: new Uri(configuration["OpenRouter:Endpoint"]!),
        apiKey: configuration["OpenRouter:ApiKey"]!,
        serviceId: "openrouter-claude-sonnet-3.7");
    
Kernel kernel = kernelBuilder.Build();

IChatCompletionService azureChatCompletionService
    = kernel.GetRequiredService<IChatCompletionService>("azure-gpt-4o");

IChatCompletionService claudSonnetChatCompletionService
    = kernel.GetRequiredService<IChatCompletionService>("openrouter-claude-sonnet-3.7");

OpenAIPromptExecutionSettings settings = new()
{
    MaxTokens = 100, Temperature = 1, TopP = 1, FrequencyPenalty = 0, PresencePenalty = 0,
    // It's also possible to set serviceId here instead
};

ChatHistory history = new();
history.AddSystemMessage("You are a helpful assistant; all your answers should rhyme.");
history.AddUserMessage("Provide a short fun fact about the moon.");

Console.Clear();

Console.Write("\n\nAn interesting fact about the moon - brought to you gpt-4o from Azure OpenAI:\n\n");

await foreach (var content in azureChatCompletionService.GetStreamingChatMessageContentsAsync(history))
{
    Console.Write(content.Content);
}

Console.Write("\n\n*********\n\nAn interesting fact about the moon - brought to you Claude Sonnet 3.7 via OpenRouter:\n\n\n");

await foreach (var content in claudSonnetChatCompletionService.GetStreamingChatMessageContentsAsync(history))
{
    Console.Write(content.Content);
}

Console.Write("\n\n");

