using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using consoleapp.basicsemantickernel.plugins;
using System.Text.Json;

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

// Create the kernel builder and add the OpenAI chat completion service
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: openAiKey,
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);

// Add and configure the logging services
kernelBuilder.Services.AddLogging(loggingBuilder =>
{
    // Configure logging providers (e.g., add console output)
    loggingBuilder.AddConsole();
    
    // Optional: Set the minimum log level (e.g., Information, Debug, Trace)
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});

// Register plugins
kernelBuilder.Plugins.AddFromType<Weather>();
kernelBuilder.Plugins.AddFromType<UserCity>();

// Build the kernel
Kernel kernel = kernelBuilder.Build();

// Get the chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create a chat history to maintain the conversation context
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

// history.AddSystemMessage("""
//             You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
//             You introduce yourself and ask questions to understand what they are looking for.
//             When helping people out, you always ask them about their preferences such as:
//             - Location (state, region)
//             - Difficulty level (easy, moderate, hard)
//             - Distance (short, medium, long)
//             - Scenery type (mountain, forest, coastal, desert)

//             You will then provide three hike suggestions based on the user's preferences. you will also share an interesting fact about each hike.
//             At the end of your response, you will remind the user to check trail conditions and prepare adequately before heading out.
//             Ask also if there is anything else you can help with.
//             """);

// while(true)
// {
//     Console.Write("user >>> ");
//     string userInput = Console.ReadLine() ?? string.Empty;
//     if (string.IsNullOrWhiteSpace(userInput))
//     {
//         break;
//     }
//     history.AddUserMessage(userInput);
    
//     Console.WriteLine("assistant >>> ");
//     var fullResponseBuilder = new System.Text.StringBuilder();

//     var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
//         chatHistory: history,
//         kernel: kernel
//     );

//     await foreach (var chunk in response)
//     {
//         Console.Write(chunk);
//         fullResponseBuilder.Append(chunk);
//     }

//     history.AddAssistantMessage(fullResponseBuilder.ToString());
//     Console.WriteLine();
// }
#endregion

#region Plugin Example Choice Behavior: Auto
//  Uncomment this section to see plugin example

// In this mode, the AI model decides whether to call a function or not, and the kernel automatically invokes it. 

// PromptExecutionSettings settingsAuto = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
// };
// history.AddUserMessage("What's the weather like for Pat?");
// var resultAuto = await chatCompletionService.GetChatMessageContentAsync(
//     chatHistory: history,
//     executionSettings: settingsAuto,
//     kernel: kernel
// );

// Console.WriteLine("Auto Result: " + resultAuto.Content);

#endregion

#region Plugin Example Choice Behavior: Required
//  Uncomment this section to see plugin example

// This forces the AI model to call at least one function from the available options. The model will not generate a text response until the function is called

// KernelFunction getUserCityFunc = kernel.Plugins.GetFunction("UserCity", "get_user_city");;

// PromptExecutionSettings settingsRequired = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions: [getUserCityFunc])
// };
// history.AddUserMessage("Tell me where Michael lives.");
// var resultRequired = await chatCompletionService.GetChatMessageContentAsync(
//     chatHistory: history,
//     executionSettings: settingsRequired,
//     kernel: kernel
// );

// Console.WriteLine("Required Result: " + resultRequired.Content);

#endregion

#region Plugin Example Choice Behavior: None
//  Uncomment this section to see plugin example

// The model is aware of the functions but is instructed not to call any of them automatically. It will provide a text response, potentially describing how it would use a function

// PromptExecutionSettings settingsNone = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.None()
// };

// history.AddUserMessage("Tell me where Mel lives");
// var resultNone = await chatCompletionService.GetChatMessageContentAsync(
//     chatHistory: history,
//     executionSettings: settingsNone,
//     kernel: kernel
// );

// Console.WriteLine("Result: " + resultNone.Content);
#endregion

#region Plugin Example Manual Invocation
//  Uncomment this section to see plugin example

// Instead of relying on automatic invocation, you can manually inspect the model's response for a function call request and invoke the function yourself.

// history.AddUserMessage("What's the weather like for Pat?");

// // Use FunctionChoiceBehavior.Auto(autoInvoke: false) or configure the settings to not auto-invoke
// // The model will still select a function, but the kernel won't invoke it
// PromptExecutionSettings settingsManual = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false) 
// };

// while (true)
// {
//     // Get the model's response
//     var resultManual = await chatCompletionService.GetChatMessageContentAsync(
//         chatHistory: history,
//         executionSettings: settingsManual,
//         kernel: kernel
//     );

//     // Check if the model returned a final text response
//     if(resultManual.Content is not null)
//     {
//         // If the model returned a text response, print it and exit the loop
//         Console.WriteLine("Final Response from the model: " + resultManual.Content);
//         break;
//     }

//     // Adding AI model response containing chosen functions to chat history as it's required by the models to preserve the context.
//     history.Add(resultManual); 
    
//     // Check if the model requested a function call
//     if (resultManual.Content is null && resultManual.Items.OfType<FunctionCallContent>().Any())
//     {
//         Console.WriteLine("AI requested a function call. Invoking manually...");
//         IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(resultManual);
//         foreach (FunctionCallContent functionCall in functionCalls)
//         {
//             try
//             {
//                 // Manually invoke the function call
//                 FunctionResultContent functionResult = await functionCall.InvokeAsync(kernel);
//                 var functionResponseMessage = functionResult.ToChatMessage();
//                 history.Add(functionResponseMessage);
//             }
//             catch (Exception ex)
//             {
//                 // Add the error message to the history if the function invocation fails
//                 history.Add(new FunctionResultContent(functionCall, ex.Message).ToChatMessage());
//             }
//         }
//     }
// }

#endregion

#region Plugin Sequential Example
// //  Uncomment this section to see plugin example

// PromptExecutionSettings settingsAuto = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
// };

// // Adding multiple questions in a single user message
// history.AddUserMessage("What's the weather in Marikina and where does Pat live?");
// // uncomment the line below to see another example reverse order of questions
// // history.AddUserMessage("where does Pat live and What's the weather in Marikina?");
// var resultAuto = await chatCompletionService.GetChatMessageContentAsync(
//     chatHistory: history,
//     executionSettings: settingsAuto,
//     kernel: kernel
// );

// Console.WriteLine("Auto Result: " + resultAuto.Content);
#endregion

#region Plugin Parallel Example

// //  Uncomment this section to see plugin example

// PromptExecutionSettings settingsParallel = new()
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
//         options: new FunctionChoiceBehaviorOptions { AllowConcurrentInvocation = true, AllowParallelCalls = true }
//     )
// };

// // Adding multiple questions in a single user message
// history.AddUserMessage("What's the weather in Marikina and where does Pat live?");
// // uncomment the line below to see another example reverse order of questions
// // history.AddUserMessage("where does Pat live and What's the weather in Marikina?");
// var resultAuto = await chatCompletionService.GetChatMessageContentAsync(
//     chatHistory: history,
//     executionSettings: settingsParallel,
//     kernel: kernel
// );

// Console.WriteLine("Auto Result: " + resultAuto.Content);

#endregion













