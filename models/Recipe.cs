// File: Models/Recipe.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        public string? InstructionsSummary { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsApproved { get; set; }
        public bool IsDisabled { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
        public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<MealPlanItem> MealPlanItems { get; set; } = new List<MealPlanItem>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
