A simple webpage where you can load some ingredients and ChatGPT will give you some recommendations!

- This app needs MySql and an OpenAI Api Key to work. You just need to create a credentials.json file inside the "RecipesRecommendations" folder like the example above with your corresponding info. 

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
