using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace AgentFramework.Factory.TestConsole.Tools.Samples;

/// <summary>
/// Sample weather tools for demonstration
/// </summary>
public class WeatherTools
{
    private readonly ILogger<WeatherTools> logger;

    public WeatherTools(ILogger<WeatherTools> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Description("Gets the current weather for a specified location. Returns temperature in Celsius, conditions, and humidity.")]
    public string GetCurrentWeather(
        [Description("The city name or location to get weather for")] string location)
    {
        logger.LogInformation("Getting current weather for location: {Location}", location);
        
        // This is a mock implementation for demonstration
        // In a real scenario, this would call a weather API
        
        var random = new Random();
        var temperature = random.Next(-10, 35);
        var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Snowy" };
        var condition = conditions[random.Next(conditions.Length)];
        var humidity = random.Next(30, 90);

        var result = $"Weather in {location}: {temperature}°C, {condition}, Humidity: {humidity}%";
        logger.LogDebug("Weather result for {Location}: {Result}", location, result);
        
        return result;
    }

    [Description("Gets a weather forecast for the next specified number of days for a location.")]
    public string GetWeatherForecast(
        [Description("The city name or location to get forecast for")] string location,
        [Description("Number of days to forecast (1-10)")] int days = 5)
    {
        logger.LogInformation("Getting weather forecast for {Location} for {Days} days", location, days);
        
        if (days < 1 || days > 10)
        {
            logger.LogWarning("Invalid forecast days requested: {Days}", days);
            return "Error: Forecast days must be between 1 and 10";
        }

        var forecast = new List<string> { $"Weather forecast for {location} ({days} days):" };
        var random = new Random();

        for (int i = 1; i <= days; i++)
        {
            var temperature = random.Next(-10, 35);
            var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Snowy" };
            var condition = conditions[random.Next(conditions.Length)];
            
            forecast.Add($"  Day {i}: {temperature}°C, {condition}");
        }

        logger.LogDebug("Generated {Days}-day forecast for {Location}", days, location);
        return string.Join("\n", forecast);
    }

    [Description("Provides clothing recommendations based on the current temperature and weather conditions.")]
    public string GetClothingRecommendation(
        [Description("Temperature in Celsius")] int temperature,
        [Description("Weather condition (e.g., Sunny, Rainy, Snowy)")] string condition = "Sunny")
    {
        logger.LogInformation("Getting clothing recommendations for {Temperature}°C and {Condition} weather", temperature, condition);
        
        var recommendations = new List<string>();

        if (temperature < 0)
        {
            recommendations.Add("Heavy winter coat");
            recommendations.Add("Warm gloves and hat");
            recommendations.Add("Insulated boots");
        }
        else if (temperature < 10)
        {
            recommendations.Add("Jacket or sweater");
            recommendations.Add("Long pants");
            recommendations.Add("Closed-toe shoes");
        }
        else if (temperature < 20)
        {
            recommendations.Add("Light jacket or cardigan");
            recommendations.Add("Comfortable casual wear");
        }
        else
        {
            recommendations.Add("Light clothing");
            recommendations.Add("T-shirt and shorts");
            recommendations.Add("Sandals or light shoes");
        }

        if (condition.Contains("Rain", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Add("Umbrella or raincoat");
            recommendations.Add("Waterproof footwear");
        }
        else if (condition.Contains("Snow", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Add("Waterproof winter boots");
            recommendations.Add("Extra warm layers");
        }

        logger.LogDebug("Generated {Count} recommendations for {Temperature}°C", recommendations.Count, temperature);
        return $"Clothing recommendations for {temperature}°C and {condition} weather:\n- " + 
               string.Join("\n- ", recommendations);
    }
}
