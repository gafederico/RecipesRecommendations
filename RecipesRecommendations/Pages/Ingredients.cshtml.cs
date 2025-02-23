using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecipesRecommendations.Database;
using RecipesRecommendations.Models;
using RecipesRecommendations.Services;
using System.Globalization;

namespace RecipesRecommendations.Pages
{
    public class IngredientsModel : PageModel
    {
        // Bind the property so that form data is automatically mapped to it
        [BindProperty]
        public required IngredientModel NewIngredient { get; set; }
        private readonly ApplicationDbContext _context;
        public required IList<IngredientModel> Ingredients;
        private readonly OpenAiService _openAiService;

        public IngredientsModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _openAiService = new OpenAiService(configuration["OpenAi:ApiKey"]);
            PopulateIngredients();
        }

        public void PopulateIngredients()
        {
            Ingredients = _context.Ingredients
                .Select(i =>
                new IngredientModel
                {
                    IdIngredient = i.IdIngredient,
                    IngredientName = i.IngredientName,
                    Amount = i.Amount
                }).ToList();
        }

        public IActionResult OnGet()
        {
            NewIngredient = new IngredientModel();
            return Page();
        }

        // Adding an ingredient
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // In case the ingredient exists, we update the amount
            var normalizedName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(NewIngredient.IngredientName.Trim().ToLower());
            var existingIngredient = Ingredients.FirstOrDefault(i => string.Equals(i.IngredientName, normalizedName, StringComparison.InvariantCultureIgnoreCase));
            if (existingIngredient != null)
            {
                existingIngredient.Amount += NewIngredient.Amount;
                _context.Ingredients.Update(existingIngredient.AsIngredient());
                await _context.SaveChangesAsync();
            }
            // If it doesn't exist, we validate before adding it
            else if (!await ValidateIngredient(normalizedName))
            {
                ModelState.AddModelError("NewIngredient.IngredientName", "The ingredient is not valid. (English only)");
            }
            else
            {
                _context.Ingredients.Add(NewIngredient.AsIngredient());
                await _context.SaveChangesAsync();
                PopulateIngredients();
            }

            return Page();
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

        private async Task<bool> ValidateIngredient(string ingredientName)
        {
            var prompt = $"Answer only with \"true\" or \"false\", nothing else, not even a dot. Is this a valid english ingredient name, edible and can be added to a recipe? : {ingredientName}";
            var isValidString = await _openAiService.GetCompletionAsync(prompt);
            bool.TryParse(isValidString.Trim(), out bool isValid);
            return isValid;
        }
    }
}
