using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;

Console.WriteLine("Let's go! App started...");
var basepath = Directory.GetCurrentDirectory();
Console.WriteLine($"Current Directory: {basepath}");
Console.WriteLine();


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("secret.json", optional: false, reloadOnChange: true)
    .Build();

string openAiKey = config["OpenAI_ApiKey"]!;

Console.WriteLine($"Your key is loaded: {(!string.IsNullOrEmpty(openAiKey) ? "Yes" : "No")}");


IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: openAiKey,
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);

Kernel kernel = kernelBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory history = [];


#region Non Streaming Example
//  Uncomment this section to see non-streaming example

// history.AddUserMessage("Explain semantic kernel in simple terms and one sentence.");
// var response = await chatCompletionService.GetChatMessageContentAsync(
//     history,
//     kernel: kernel
// );

// Console.WriteLine("Response from the model:");
// Console.WriteLine(response);
#endregion

#region Streaming Example
//  Uncomment this section to see streaming example

// history.AddUserMessage("Explain semantic kernel in simple terms and one sentence.");
// var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
//     chatHistory: history,
//     kernel: kernel
// );

// await foreach (var chunk in response)
// {
//     Console.Write(chunk);
// }
#endregion

#region Chat Context Example
// //  Uncomment this section to see chat context example

history.AddSystemMessage("""
            You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
            You introduce yourself and ask questions to understand what they are looking for.
            When helping people out, you always ask them about their preferences such as:
            - Location (state, region)
            - Difficulty level (easy, moderate, hard)
            - Distance (short, medium, long)
            - Scenery type (mountain, forest, coastal, desert)

            You will then provide three hike suggestions based on the user's preferences. you will also share an interesting fact about each hike.
            At the end of your response, you will remind the user to check trail conditions and prepare adequately before heading out.
            Ask also if there is anything else you can help with.
            """);

while(true)
{
    Console.Write("user >>> ");
    string userInput = Console.ReadLine() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }
    history.AddUserMessage(userInput);
    
    Console.WriteLine("assistant >>> ");
    var fullResponseBuilder = new System.Text.StringBuilder();

    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatHistory: history,
        kernel: kernel
    );

    await foreach (var chunk in response)
    {
        Console.Write(chunk);
        fullResponseBuilder.Append(chunk);
    }

    history.AddAssistantMessage(fullResponseBuilder.ToString());
    Console.WriteLine();
}
#endregion