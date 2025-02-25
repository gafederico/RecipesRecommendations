using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecipesRecommendations.Database;
using RecipesRecommendations.ViewModels;
using RecipesRecommendations.Services;
using System.Globalization;

namespace RecipesRecommendations.Pages
{
    public class IngredientsModel : PageModel
    {
        // Bind the property so that form data is automatically mapped to it
        [BindProperty]
        public required IngredientViewModel NewIngredient { get; set; }
        private readonly ApplicationDbContext _context;
        public required IList<IngredientViewModel> Ingredients;
        private readonly OpenAiService _openAiService;
        private readonly ILogger<IndexModel> _logger;

        public IngredientsModel(ApplicationDbContext context, ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            try
            {
                _openAiService = new OpenAiService(configuration["OpenAi:ApiKey"]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing OpenAI service: {ex.Message}");
            }
            PopulateIngredients();
        }

        public IActionResult OnGet()
        {
            NewIngredient = new IngredientViewModel();
            return Page();
        }

        // Adding an ingredient
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // In case the ingredient exists, we update the amount
                NewIngredient.IngredientName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(NewIngredient.IngredientName.Trim().ToLower());
                var existingIngredient = Ingredients.FirstOrDefault(i => string.Equals(i.IngredientName, NewIngredient.IngredientName, StringComparison.InvariantCultureIgnoreCase));
                if (existingIngredient != null)
                {
                    existingIngredient.Amount += NewIngredient.Amount;
                    _context.Ingredients.Update(existingIngredient.AsIngredient());
                    await _context.SaveChangesAsync();
                    ModelState.Clear();
                    NewIngredient = new IngredientViewModel();
                }
                // If it doesn't exist, we validate before adding it
                else if (await ValidateIngredient(NewIngredient.IngredientName))
                {
                    _context.Ingredients.Add(NewIngredient.AsIngredient());
                    await _context.SaveChangesAsync();
                    PopulateIngredients();
                    ModelState.Clear();
                    NewIngredient = new IngredientViewModel();
                }
                else
                    ModelState.AddModelError("NewIngredient.IngredientName", "The ingredient is not valid. (English only)");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing the request");
                ModelState.AddModelError("NewIngredient.IngredientName", $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        // Deleting an ingredient
        public async Task<IActionResult> OnPostDeleteIngredientAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
                PopulateIngredients();
            }

            return RedirectToPage();
        }

        #region Helpers

        /// <summary>
        /// Validates with ChatGPT if the ingredient is valid.
        /// TODO: This should be a list of valid ingredients from the database, and the user should
        /// pick a valid ingredient. Maybe use this as a method to validate new ingredients in another page.
        /// </summary>
        /// <param name="ingredientName">The name of the ingredient</param>
        /// <returns>True if the ingredient is in english, is edible and can be used in a recipe</returns>
        private async Task<bool> ValidateIngredient(string ingredientName)
        {
            var prompt = $"Answer only with \"true\" or \"false\". Is this a valid english ingredient name, edible and can be added to a recipe? : {ingredientName}";
            var isValidString = await _openAiService.GetCompletionAsync(prompt);
            bool.TryParse(isValidString.TrimEnd(' ', '.').ToLowerInvariant(), out bool isValid);
            return isValid;
        }

        private void PopulateIngredients()
        {
            Ingredients = _context.Ingredients
                .Select(i =>
                new IngredientViewModel
                {
                    IdIngredient = i.IdIngredient,
                    IngredientName = i.IngredientName,
                    Amount = i.Amount
                }).ToList();
        }

        #endregion
    }
}
