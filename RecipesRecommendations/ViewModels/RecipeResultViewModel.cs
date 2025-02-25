using System.Text.Json.Serialization;

namespace RecipesRecommendations.ViewModels
{
    public class RecipeResultViewModel
    {
        [JsonPropertyName("ingredients")]
        public List<IngredientViewModel> Ingredients { get; set; }
        [JsonPropertyName("htmlRaw")]
        public string HtmlRaw { get; set; }
    }
}
