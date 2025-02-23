using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RecipesRecommendations.Database
{
    public class Ingredient
    {
        [Key]
        public int IdIngredient { get; set; }
        public required string IngredientName { get; set; }
        public int Amount { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }
    }
}
