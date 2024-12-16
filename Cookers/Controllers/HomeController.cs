using Cookers.Data;
using Cookers.Models;
using CookMaster.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace Cookers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Retrieve the first 5 recipe images
            var recipeImages = _context.Recipes
                                       .OrderBy(r => r.Id)
                                       .Take(5)
                                       .Select(r => r.ImageUrl)
                                       .ToList();

            // If no recipes, add a placeholder image
            if (!recipeImages.Any())
            {
                recipeImages.Add("/images/placeholder.png"); // Path to your placeholder image
            }

            // Fetch the latest 5 recipes (including their Id, Name, and Description for the clickable images)
            var freshRecipes = _context.Recipes
                                       .OrderByDescending(r => r.Id)
                                       .Take(5)
                                       .ToList();

            // Fetch the top 3 recipes with the highest average ratings
            var topRatedRecipes = _context.Recipes
                                          .Select(r => new
                                          {
                                              Recipe = r,
                                              AverageRating = _context.Ratings
                                                                      .Where(rt => rt.RecipeId == r.Id)
                                                                      .Average(rt => (double?)rt.Value) ?? 0
                                          })
                                          .OrderByDescending(r => r.AverageRating)
                                          .Take(3)
                                          .ToList();

            // Store the average rating in ViewData for all recipes (both fresh and top-rated)
            foreach (var recipe in freshRecipes)
            {
                var averageRating = _context.Ratings
                    .Where(r => r.RecipeId == recipe.Id)
                    .Average(r => (double?)r.Value);

                ViewData[$"AvgRating_{recipe.Id}"] = averageRating;
            }

            // Store the top-rated recipes to pass to the view
            ViewData["TopRatedRecipes"] = topRatedRecipes.Select(r => r.Recipe).ToList();

            ViewData["RecipeImages"] = recipeImages;

            return View(freshRecipes); // Passing fresh recipes to the view
        }




        public IActionResult LifestyleDetail(int id)
        {
            var lifestyleTip = id switch
            {
                1 => new { Title = "Healthy Eating", Content = "Discover nutritious recipes and tips for balanced meals.", ImageUrl = "https://jackcityfitness.com/wp-content/uploads/shutterstock_1351783832-1.jpg" },
                2 => new { Title = "Active Living", Content = "Incorporate more movement into your daily routine.", ImageUrl = "https://nlphysio.com/media/img-4-tips-to-help-you-lead-a-more-healthy-active-lifestyle.jpg" },
                3 => new { Title = "Mental Wellbeing", Content = "Tips for stress management and mental health awareness.", ImageUrl = "https://whws.org.au/wp-content/uploads/2021/12/get-on-top-of-mental-health-early.jpg" },
                4 => new { Title = "Stay Hydrated", Content = "The importance of drinking water throughout the day.", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSJS8uRwl6QQ3i7ur0xhTWr7GLnKvCgFPNSqg&s" },
                _ => new { Title = "Unknown", Content = "This tip is not available.", ImageUrl = "/images/placeholder.jpg" }
            };

            ViewData["Title"] = lifestyleTip.Title;
            ViewData["Content"] = lifestyleTip.Content;
            ViewData["ImageUrl"] = lifestyleTip.ImageUrl;

            return View(); // This will return the LifestyleDetail view
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
