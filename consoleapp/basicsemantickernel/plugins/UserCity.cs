using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
namespace consoleapp.basicsemantickernel.plugins;

public class UserCity
{
    private readonly ILogger<UserCity> _logger;

    public UserCity(ILogger<UserCity> logger)
    {
        _logger = logger;
    }

    [KernelFunction("get_user_city")]
    [Description("Get the city of a user by their name")]
    public async Task<string> GetUserCity(string name)
    {
        _logger.LogInformation("Start Fetching city for user: {UserName}", name);
        // Simulate an asynchronous operation to fetch city info
        await Task.Delay(500); // Simulating delay
                var city = name switch
        {
            "Mel" => "Marikina",
            "Pat" => "Taguig",
            "Michael" => "Bulacan",
            _ => "Not Defined",
        };
        _logger.LogInformation("Finished fetching information for city: {CityName}", city);
        return city;
    }
}