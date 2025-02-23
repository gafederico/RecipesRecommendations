using RecipesRecommendations.Database;
using System.ComponentModel.DataAnnotations;

namespace RecipesRecommendations.Models
{
    public class IngredientModel
    {
        public int IdIngredient { get; set; }
        [Display(Name = "Ingredient Name")]
        public string IngredientName { get; set; }
        public int Amount { get; set; }
        public IngredientModel()
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
