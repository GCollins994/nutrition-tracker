using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nutrition_Tracker.Models;
using Nutrition_Tracker.Utilities;

namespace Nutrition_Tracker.Services;

public class NutritionApiService
{
    private readonly ILogger<NutritionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ApiKeys _apiKeys;
    
    public NutritionApiService(ILogger<NutritionService> logger, HttpClient httpClient, IOptions<ApiKeys> apiKeys)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKeys = apiKeys.Value;
        
        _logger.LogInformation($"API Key: {_apiKeys.FdcApiKey}"); // Debugging line
    }

    /// <summary>
    /// Search the Food Data Central API for food items. Input a search term and get a list of food items.
    /// </summary>
    /// <param name="searchTerm">Keyword</param>
    /// <param name="itemsPerPage">50/100/150/200</param>
    /// <param name="pageNumber"></param>
    /// <returns>List of FoodSearchModel</returns>
    public async Task<FoodSearchModel> SearchFoodApiRequest(string input, Enums.SearchItemsPerPage itemsPerPage, int pageNumber)
    {
        try
        {
            var url = $"{Constants.fdcBaseUrlPrefix}foods/search?query={input}&api_key={_apiKeys.FdcApiKey}&pageSize={(int)itemsPerPage}&pageNumber={pageNumber}";
                
            var response = await PollyPolicies.HttpRequestRetry.ExecuteAsync(() =>
                _httpClient.GetAsync(url)
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching data from API: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
        
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FoodSearchModel>(content);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError($"Exception occurred while fetching data from API: {e.Message}", e);
            throw;
        }
    }
}