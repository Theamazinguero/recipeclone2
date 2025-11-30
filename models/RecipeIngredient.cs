// File: Models/RecipeIngredient.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Quantity { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }
    }
}
