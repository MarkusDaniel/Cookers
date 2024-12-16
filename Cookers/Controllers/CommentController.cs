using Cookers.Data;
using Cookers.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cookers.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int recipeId, string content)
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Invalid request or unauthorized." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the user has already commented on this recipe
            if (_context.Comments.Any(c => c.RecipeId == recipeId && c.UserId == userId))
            {
                return Json(new { success = false, message = "You have already commented on this recipe." });
            }

            var comment = new Comment
            {
                RecipeId = recipeId,
                UserId = userId,
                Content = content.Trim(),
                DateCreated = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                comment = new
                {
                    content = comment.Content,
                    dateCreated = comment.DateCreated.ToString("g")
                }
            });
        }

        [HttpGet]
        public IActionResult GetRecipeComments(int recipeId)
        {
            var comments = _context.Comments
                .Where(c => c.RecipeId == recipeId)
                .OrderByDescending(c => c.DateCreated)
                .Select(c => new
                {
                    c.Content,
                    DateCreated = c.DateCreated.ToString("g")
                })
                .ToList();

            return Json(comments);
        }
    }
}
