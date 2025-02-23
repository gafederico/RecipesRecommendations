using OpenAI.Chat;

namespace RecipesRecommendations.Services
{
    public class OpenAiService
    {
        private readonly string _apiKey;
        private readonly string _gptModel = "chatgpt-4o-latest";

        public OpenAiService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            ChatClient client = new(model: _gptModel, apiKey: _apiKey);

            ChatCompletion completion = await client.CompleteChatAsync(prompt);

            return completion.Content[0].Text ?? "";
        }
    }

}
