// File: Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace RecipeApp.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsBanned { get; set; }

        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    }
}
