// File: Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Data;
using RecipeApp.Api.Models;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ----- Recipe Approval Queue -----
        [HttpGet("recipes/pending")]
        public async Task<ActionResult> GetPendingRecipes()
        {
            var pending = await _db.Recipes
                .Include(r => r.CreatedByUser)
                .Where(r => !r.IsApproved && !r.IsDisabled)
                .ToListAsync();

            var result = pending.Select(r => new
            {
                r.Id,
                r.Title,
                Author = r.CreatedByUser!.DisplayName,
                r.CreatedAtUtc
            });

            return Ok(result);
        }

        [HttpPost("recipes/{id:int}/approve")]
        public async Task<IActionResult> ApproveRecipe(int id)
        {
            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            recipe.IsApproved = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("recipes/{id:int}/disable")]
        public async Task<IActionResult> DisableRecipe(int id)
        {
            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            recipe.IsDisabled = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ----- Reports moderation -----
        [HttpGet("reports")]
        public async Task<ActionResult> GetReports()
        {
            var reports = await _db.Reports
                .Include(r => r.Recipe)
                .Include(r => r.Comment)
                .Include(r => r.ReporterUser)
                .Where(r => !r.IsResolved)
                .ToListAsync();

            var result = reports.Select(r => new
            {
                r.Id,
                r.Reason,
                Reporter = r.ReporterUser!.DisplayName,
                RecipeId = r.RecipeId,
                CommentId = r.CommentId,
                r.CreatedAtUtc
            });

            return Ok(result);
        }

        [HttpPost("reports/{id:int}/resolve")]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _db.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ----- Comments moderation -----
        [HttpPost("comments/{id:int}/hide")]
        public async Task<IActionResult> HideComment(int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            comment.IsHidden = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ----- User management (warn / ban) -----
        [HttpPost("users/{userId}/ban")]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsBanned = true;
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        // A "warn" could just be a placeholder now (in a real app you’d log or email)
        [HttpPost("users/{userId}/warn")]
        public async Task<IActionResult> WarnUser(string userId, [FromBody] string message)
        {
            // TODO: send email or notification
            var exists = await _userManager.FindByIdAsync(userId);
            if (exists == null) return NotFound();

            // For now just 200 OK
            return Ok(new { warnedUserId = userId, message });
        }

        // ----- Tags: merge duplicates -----
        [HttpPost("tags/merge")]
        public async Task<IActionResult> MergeTags([FromQuery] int fromTagId, [FromQuery] int intoTagId)
        {
            if (fromTagId == intoTagId) return BadRequest("Tags must be different.");

            var fromTag = await _db.Tags.Include(t => t.RecipeTags).FirstOrDefaultAsync(t => t.Id == fromTagId);
            var intoTag = await _db.Tags.Include(t => t.RecipeTags).FirstOrDefaultAsync(t => t.Id == intoTagId);

            if (fromTag == null || intoTag == null) return NotFound();

            // Reassign recipe tags
            foreach (var rt in fromTag.RecipeTags)
            {
                if (!intoTag.RecipeTags.Any(x => x.RecipeId == rt.RecipeId))
                {
                    intoTag.RecipeTags.Add(new RecipeTag
                    {
                        RecipeId = rt.RecipeId,
                        TagId = intoTag.Id
                    });
                }
            }

            _db.Tags.Remove(fromTag);
            await _db.SaveChangesAsync();

            return Ok();
        }

        // ----- Simple "analytics" -----
        [HttpGet("analytics/overview")]
        public async Task<IActionResult> GetOverviewAnalytics()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalRecipes = await _db.Recipes.CountAsync();
            var totalApprovedRecipes = await _db.Recipes.CountAsync(r => r.IsApproved && !r.IsDisabled);
            var totalReports = await _db.Reports.CountAsync();
            var totalComments = await _db.Comments.CountAsync();

            return Ok(new
            {
                totalUsers,
                totalRecipes,
                totalApprovedRecipes,
                totalComments,
                totalReports
            });
        }
    }
}
