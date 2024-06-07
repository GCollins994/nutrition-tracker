using System.Reflection.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nutrition_Tracker.Interfaces;
using Nutrition_Tracker.Models;
using Nutrition_Tracker.Utilities;

namespace Nutrition_Tracker.Services;

public class NutritionApiService
{
    private readonly ILogger<NutritionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ApiKeys _apiKeys;
    private readonly IMemoryCache _cacheService;
    private readonly INutritionValueParser _nutritionValueParser;
    
    public NutritionApiService(ILogger<NutritionService> logger, HttpClient httpClient, IOptions<ApiKeys> apiKeys,
        IMemoryCache cacheService, INutritionValueParser nutritionValueParser)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKeys = apiKeys.Value;
        _cacheService = cacheService;
        _nutritionValueParser = nutritionValueParser;
        
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

    /// <summary>
    /// Get Food Data Central API Nutrition Values. Input fdcId and get a list of values. Using NutritionValuesModel,
    /// and Parsing the data using NutritionValuesParser.
    /// </summary>
    /// <param name="fdcId">Food Data Central API - Item ID</param>
    /// <returns></returns>
    public async Task<NutritionValuesModel> GetNutritionValueApiRequest(string fdcId)
    {
        try
        {
            var url = $"{Constants.fdcBaseUrlPrefix}food/{fdcId}?api_key={_apiKeys.FdcApiKey}";

            var response = await PollyPolicies.HttpRequestRetry.ExecuteAsync(() =>
                _httpClient.GetAsync(url)
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching data from API: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Log the response content for debugging
            _logger.LogInformation($"API Response: {content}");

            // Ensure the response content is not null or empty
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogError("API response content is null or empty");
                return null;
            }
            
            return _nutritionValueParser.ParseNutrition(content);
        }
        catch (Exception e)
        {
            _logger.LogError($"Exception occurred while fetching data from API: {e.Message}", e);
            throw;
        }
    }
}