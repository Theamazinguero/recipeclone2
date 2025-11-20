// File: Models/Tag.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(60)]
        public string Slug { get; set; } = string.Empty;

        public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
    }
}
