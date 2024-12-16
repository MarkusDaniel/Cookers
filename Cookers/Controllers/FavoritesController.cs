using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cookers.Data;
using CookMaster.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Cookers.Models.CookMaster.Models;

namespace CookMaster.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View all favorites for the current user
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var favoriteRecipes = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.RecipeId)
                .ToListAsync();

            var recipes = await _context.Recipes
                .Where(r => favoriteRecipes.Contains(r.Id))
                .ToListAsync();

            return View(recipes);
        }

        // Add a recipe to favorites
        [HttpPost]
        public async Task<IActionResult> Add(int recipeId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {
                // Ensure it doesn't already exist
                if (!await _context.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId))
                {
                    var favorite = new Favorite
                    {
                        UserId = userId,
                        RecipeId = recipeId
                    };

                    _context.Favorites.Add(favorite);
                    await _context.SaveChangesAsync();
                }
            }

            // Stay on the same page without redirecting
            return NoContent();
        }



        // Remove a recipe from favorites
        [HttpPost]
        public async Task<IActionResult> Remove(int recipeId)
        {
            var userId = _userManager.GetUserId(User);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
