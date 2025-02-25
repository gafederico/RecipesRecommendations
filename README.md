A simple webpage where you can load some ingredients and ChatGPT will give you some recommendations!

There's two main pages:
- **Ingredients**: Where you can create your ingredients, set an amount for each, or delete any ingredients you don't have anymore.
- **Home**: Where the recipe recommendations are. After getting a recommendation, you can ask for another recommendation or to confirm the recipe, and update the Ingredients table based on the ingredients used.

This app needs **MySql** and an **OpenAI Api Key** to work. You just need to create a credentials.json file inside the "RecipesRecommendations" folder like the example above with your corresponding info. 
```json
{
  "OpenAi": {
    "ApiKey": "YourOpenAIApiKeyHere"
  },
  "ConnectionStrings": {
    "DefaultConnection": "YourConnectionStringInfohere;database=RecipesRecommendations"
  }
}
```

(Note: It uses [Pomelo](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) to connect to MySQL, and it will create automatically the "RecipesRecommendations" Database in case it doesn't exist)
