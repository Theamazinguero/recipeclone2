// File: Controllers/MealPlansController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Data;
using RecipeApp.Api.DTOs;
using RecipeApp.Api.Models;
using System.Security.Claims;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MealPlansController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MealPlansController(AppDbContext db)
        {
            _db = db;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MealPlan>>> GetMyMealPlans()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var plans = await _db.MealPlans
                .Include(mp => mp.Items)
                .Where(mp => mp.UserId == userId)
                .ToListAsync();

            return Ok(plans);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMealPlan(MealPlanCreateRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var plan = new MealPlan
            {
                UserId = userId,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date
            };

            foreach (var item in request.Items)
            {
                plan.Items.Add(new MealPlanItem
                {
                    RecipeId = item.RecipeId,
                    Date = item.Date.Date,
                    MealType = item.MealType
                });
            }

            _db.MealPlans.Add(plan);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyMealPlans), new { }, null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMealPlan(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var plan = await _db.MealPlans.FindAsync(id);
            if (plan == null) return NotFound();
            if (plan.UserId != userId) return Forbid();

            _db.MealPlans.Remove(plan);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
