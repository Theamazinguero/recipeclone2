// File: Models/RecipeStep.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class RecipeStep
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int StepNumber { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}
