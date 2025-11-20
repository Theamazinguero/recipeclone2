// File: Models/MealPlan.cs
namespace RecipeApp.Api.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<MealPlanItem> Items { get; set; } = new List<MealPlanItem>();
    }
}
