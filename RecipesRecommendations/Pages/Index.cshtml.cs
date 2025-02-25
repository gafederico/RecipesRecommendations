using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RecipesRecommendations.Database;
using RecipesRecommendations.Services;
using System.Text.Json;
using Markdig;
using System.Text.Json.Serialization.Metadata;
using RecipesRecommendations.ViewModels;
using System.Text.RegularExpressions;

namespace RecipesRecommendations.Pages
{
    public class IndexModel : PageModel
    {
        private readonly OpenAiService _openAiService;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private static string _lastRecommendation = string.Empty;
        private static IList<IngredientViewModel> _usedIngredients = new List<IngredientViewModel>();
        public string ResultText { get; set; }

        public IndexModel(IConfiguration configuration, ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            ResultText = string.Empty;

            // Instance the logger
            _logger = logger;

            // Instance the context
            _context = context;

            // Instance the OpenAIService
            try
            {
                var apiKey = configuration["OpenAi:ApiKey"];
                _openAiService = new OpenAiService(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OpenAI service");
            }
        }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Retrieve the list of ingredients from the database
                var ingredients = await _context.Ingredients.ToListAsync();

                // No ingredients available
                if (ingredients.Count == 0)
                {
                    ResultText = "No ingredients found in the database! Please load some under the Ingredients page and then retry!";
                    return Page();
                }

                // Serialize the list to JSON with optional formatting for readability
                var ingredientsVMs = ingredients.Select(i => new IngredientViewModel
                {
                    IdIngredient = i.IdIngredient,
                    IngredientName = i.IngredientName,
                    Amount = i.Amount
                }).ToList();
                var json = JsonSerializer.Serialize(ingredientsVMs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Ask ChatGPT for a recipe
                var prompt = $"Based only on the next Json with ingredients, knowing that you can use at most this ingredients but not necessary use every ingredient, give me a simple recipe. " +
                    $"{(!string.IsNullOrWhiteSpace(_lastRecommendation) ? $"This was your previous recommendation, do not give the same one: {_lastRecommendation}\n" : string.Empty)}" +
                    $"Return the result in a json format, with two parameters, " +
                    $"the first one called \"ingredients\", with the same format as the next json with the ingredients and their corresponding amounts you used," +
                    $"and the second one called \"htmlRaw\" where your response will be." +
                    $"Please return only valid JSON. Do not include any Markdown code fences like ```json." +
                    $"\n {json}";

                // Removes any possible leading ```json and trailing ```
                var rawAIResponse = await _openAiService.GetCompletionAsync(prompt);
                var aiResponse = Regex.Replace(rawAIResponse, "^```json|```$", "", RegexOptions.Multiline).Trim();

                // Deserialize the response
                var result = JsonSerializer.Deserialize<RecipeResultViewModel>(aiResponse);

                ResultText = Markdown.ToHtml(result.HtmlRaw);
                _usedIngredients = result.Ingredients;
                _lastRecommendation = ResultText;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing the request");
                ResultText = "An error occurred while processing the request. Please try again later!";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateUsedIngredientsAsync(int id)
        {
            try
            {
                // Retrieve the list of ingredients from the database
                var ingredients = await _context.Ingredients.ToListAsync();
                var usedIngredientsDB = _usedIngredients.Select(i => i.AsIngredient()).ToList();
                foreach (var usedIngredient in usedIngredientsDB)
                {
                    var actualIngredient = ingredients.First(i => i.IdIngredient == usedIngredient.IdIngredient);
                    if (actualIngredient.Amount > usedIngredient.Amount)
                    {
                        actualIngredient.Amount -= usedIngredient.Amount;
                        _context.Ingredients.Update(actualIngredient);
                    }
                    else
                        _context.Ingredients.Remove(actualIngredient);
                }
                await _context.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting the used ingredients");
                ResultText = "An error occurred while deleting the used ingredients. Please try again later!";
                return Page();
            }
        }
    }
}
