// File: Models/Favorite.cs
namespace RecipeApp.Api.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
