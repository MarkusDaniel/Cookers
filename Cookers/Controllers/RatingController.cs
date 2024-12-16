using Cookers.Data;
using CookMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cookers.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Rate a Recipe
        public async Task<IActionResult> Rate(int recipeId, int ratingValue)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Debug logs for inputs
            Console.WriteLine($"Recipe ID: {recipeId}, Rating Value: {ratingValue}");

            if (ratingValue < 1 || ratingValue > 5)
            {
                return BadRequest("Rating value must be between 1 and 5.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"User ID: {userId}");

            // Check if the user has already rated this recipe
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Value = ratingValue;
                _context.Update(existingRating);
                Console.WriteLine("Updated existing rating.");
            }
            else
            {
                var rating = new Rating
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    Value = ratingValue
                };
                _context.Ratings.Add(rating);
                Console.WriteLine("Added new rating.");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Changes saved to database.");

            return RedirectToAction("Detail", "Recipe", new { id = recipeId });
        }


        // Calculate the average rating for a recipe
        [HttpGet]
        public async Task<double> GetAverageRating(int recipeId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RecipeId == recipeId)
                .ToListAsync();

            if (!ratings.Any())
            {
                return 0; // No ratings
            }

            return ratings.Average(r => r.Value);
        }

        // Get the user's rating for a recipe
        [HttpGet]
        public async Task<int?> GetUserRating(int recipeId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

            return userRating?.Value;
        }
    }
}
