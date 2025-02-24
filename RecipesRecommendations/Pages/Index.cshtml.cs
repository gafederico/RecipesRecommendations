using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RecipesRecommendations.Database;
using RecipesRecommendations.Services;
using System.Text.Json;
using Markdig;

namespace RecipesRecommendations.Pages
{
    public class IndexModel : PageModel
    {
        private readonly OpenAiService _openAiService;
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private static string _lastRecommendation = string.Empty;
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
                var json = JsonSerializer.Serialize(ingredients, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var prompt = $"{(!string.IsNullOrWhiteSpace(_lastRecommendation) ? $"This was your previous recommendation, do not give the same one: {_lastRecommendation}\n" : string.Empty)}" +
                    $"Based only on the next Json with ingredients, knowing that you can use at most this ingredients but not necessary use every ingredient, give me a simple recipe:\n {json}";
                ResultText = Markdown.ToHtml(await _openAiService.GetCompletionAsync(prompt));
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
    }
}
