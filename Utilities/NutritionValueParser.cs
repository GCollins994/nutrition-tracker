using Newtonsoft.Json.Linq;
using Nutrition_Tracker.Interfaces;
using Nutrition_Tracker.Models;

namespace Nutrition_Tracker.Utilities;

/// <summary>
/// Class to parse the Nutrition Values Fetched from the FDC API, store them into a parsed format and return them to
/// the Controller or used elsewhere
/// </summary>
public class NutritionValueParser : INutritionValueParser
{
    public NutritionValuesModel ParseNutrition(string json)
    {
        var jsonNutrition = JObject.Parse(json);
        
        var labelNutrients = jsonNutrition["labelNutrients"].ToObject<LabelNutrients>();
        
        var nutritionValuesModel = new NutritionValuesModel
        {
            FoodId = (string)jsonNutrition["fdcId"],
            Description = (string)jsonNutrition["description"],
            FoodCategory = (string)jsonNutrition["foodCategory"],
            PortionSize = (double?)jsonNutrition["servingSize"] ?? 0,
            PortionSizeUnit = (string)jsonNutrition["servingSizeUnit"],
            LabelNutrients = labelNutrients
        };

        return nutritionValuesModel;
    }
}