// Models/MealPlan.cs

using System;
using System.Collections.Generic;

namespace RecipeBackend.Models
{
    public class MealPlan
    {
        public int Id { get; set; }

        // The user who owns this meal plan
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // Date range for the meal plan
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Primary navigation property used by EF Core
        public ICollection<MealPlanItem> Items { get; set; } = new List<MealPlanItem>();

        // Backwards-compatible alias
        // If any old code tries to use MealPlanItems, it will still work.
        public ICollection<MealPlanItem> MealPlanItems
        {
            get => Items;
            set => Items = value ?? new List<MealPlanItem>();
        }
    }
}
