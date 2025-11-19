using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
namespace consoleapp.basicsemantickernel.plugins;

public class Weather
{
    private readonly ILogger<Weather> _logger;

    public Weather(ILogger<Weather> logger)
    {
        _logger = logger;
    }

    [KernelFunction("get_weather")]
    [Description("Get the weather for a given city")]
    public async Task<string> GetWeatherAsync(string city)
    {
        _logger.LogInformation("Start Fetching weather for city: {CityName}", city);
        // Simulate an asynchronous operation to fetch weather info
        await Task.Delay(500); // Simulating delay
                var weather = city switch
        {
            "Marikina" => "Sunny, 30°C",
            "Taguig" => "Partly Cloudy, 28°C",
            "Bulacan" => "Rainy, 25°C",
            _ => "City not found",
        };
        _logger.LogInformation("Finished fetching weather for city: {CityName}", city);
        return weather;
    }
}