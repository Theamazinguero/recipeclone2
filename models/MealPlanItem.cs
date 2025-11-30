// Models/MealPlanItem.cs

using System;

namespace RecipeBackend.Models
{
    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack
    }

    public class MealPlanItem
    {
        public int Id { get; set; }

        public int MealPlanId { get; set; }
        public MealPlan? MealPlan { get; set; }

        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public DateTime Date { get; set; }
        public MealType MealType { get; set; }
    }
}
