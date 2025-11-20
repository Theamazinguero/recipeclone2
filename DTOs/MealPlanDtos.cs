// File: DTOs/MealPlanDtos.cs
using System.ComponentModel.DataAnnotations;
using RecipeApp.Api.Models;

namespace RecipeApp.Api.DTOs
{
    public class MealPlanItemRequest
    {
        [Required]
        public int RecipeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public MealType MealType { get; set; }
    }

    public class MealPlanCreateRequest
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public List<MealPlanItemRequest> Items { get; set; } = new();
    }
}
