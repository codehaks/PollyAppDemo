using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WeatherApp.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HttpClient _httpClient;

    public List<WeatherForecast> WeatherData { get; set; } = new List<WeatherForecast>();
    public bool IsLoading { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public IndexModel(ILogger<IndexModel> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task OnGetAsync()
    {
        try
        {
            IsLoading = true;
            var cities = new[] { "New York", "London", "Tokyo", "Paris", "Sydney", "Berlin", "Rome", "Moscow", "Toronto", "Dubai" };
            
            var tasks = cities.Select(async city =>
            {
                try
                {
                    var response = await _httpClient.GetAsync($"https://localhost:7211/WeatherForecast/city/{Uri.EscapeDataString(city)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var weather = JsonSerializer.Deserialize<WeatherForecast>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return weather;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching weather for {City}", city);
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            WeatherData = results.Where(w => w != null).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            ErrorMessage = "Failed to load weather data. Please try again later.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
    public string? City { get; set; }
}
