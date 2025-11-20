// File: Models/Report.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class Report
    {
        public int Id { get; set; }

        public string ReporterUserId { get; set; } = string.Empty;
        public ApplicationUser? ReporterUser { get; set; }

        public int? RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }

        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;

        public bool IsResolved { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
