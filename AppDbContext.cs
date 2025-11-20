// File: Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Models;

namespace RecipeApp.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
        public DbSet<MealPlan> MealPlans => Set<MealPlan>();
        public DbSet<MealPlanItem> MealPlanItems => Set<MealPlanItem>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Setting> Settings => Set<Setting>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RecipeTag>()
                .HasKey(rt => new { rt.RecipeId, rt.TagId });

            builder.Entity<RecipeTag>()
                .HasOne(rt => rt.Recipe)
                .WithMany(r => r.RecipeTags)
                .HasForeignKey(rt => rt.RecipeId);

            builder.Entity<RecipeTag>()
                .HasOne(rt => rt.Tag)
                .WithMany(t => t.RecipeTags)
                .HasForeignKey(rt => rt.TagId);
        }
    }
}
