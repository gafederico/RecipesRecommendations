using RecipesRecommendations.Database;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RecipesRecommendations.ViewModels
{
    public class IngredientViewModel
    {
        [JsonPropertyName("IdIngredient")]
        public int IdIngredient { get; set; }

        [Display(Name = "Ingredient Name")]
        [JsonPropertyName("IngredientName")]
        public string IngredientName { get; set; }

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }

        public IngredientViewModel()
        {
            IngredientName = string.Empty;
            Amount = 0;
        }
        /// <summary>
        /// Parses the IngredientModel to the corresponding Ingredient DB class
        /// </summary>
        /// <returns>An instance of an Ingredient DB</returns>
        public Ingredient AsIngredient()
        {
            return new Ingredient
            {
                IdIngredient = IdIngredient,
                IngredientName = IngredientName,
                Amount = Amount
            };
        }
    }
}
