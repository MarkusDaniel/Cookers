using Cookers.Data;
using CookMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cookers.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all recipes (search functionality included)
        public async Task<IActionResult> Index(string searchQuery)
        {
            ViewData["SearchQuery"] = searchQuery; // Store the search query to pre-fill the search bar

            var recipes = from r in _context.Recipes
                          select r;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                recipes = recipes.Where(r => r.Name.Contains(searchQuery));
            }

            // Fetch the fresh recipes from the database
            var recipeList = await recipes.ToListAsync();

            // Calculate the average rating for each recipe
            foreach (var recipe in recipeList)
            {
                var avgRating = _context.Ratings
                                        .Where(r => r.RecipeId == recipe.Id)
                                        .Average(r => (double?)r.Value); // Nullable to handle no ratings
                ViewData[$"AvgRating_{recipe.Id}"] = avgRating ?? 0; // If no ratings, set it to 0
            }

            return View(recipeList); // Pass the list of recipes to the view
        }

        // Display recipes owned by the logged-in user
        public async Task<IActionResult> Manage()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); // Redirect if not logged in
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID
            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(recipes);
        }

        // Display Add Recipe Form (only if logged in)
        [HttpGet]
        public IActionResult Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); // Redirect if not logged in
            }

            return View(new Recipe());
        }

        // POST the new recipe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                // Set the UserId of the logged-in user
                recipe.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                recipe.IngredientsString = string.Join(",", recipe.Ingredients);

                // Add the recipe to the database
                _context.Add(recipe);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home"); // Redirect after adding the recipe
            }
            return View(recipe); // Return the view with the recipe if there is an error
        }

        // Display the Edit Recipe Form (only if the recipe belongs to the user)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); // Redirect if not logged in
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound(); // Return 404 if recipe not found or user doesn't own it
            }

            return View(recipe);
        }

        // POST the edited recipe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingRecipe = await _context.Recipes
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

                if (existingRecipe == null)
                {
                    return NotFound(); // Prevent unauthorized updates
                }

                try
                {
                    // Update recipe
                    existingRecipe.Name = recipe.Name;
                    existingRecipe.Description = recipe.Description;
                    existingRecipe.ImageUrl = recipe.ImageUrl;
                    existingRecipe.IngredientsString = string.Join(",", recipe.Ingredients);

                    _context.Update(existingRecipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(recipe);
        }

        // Delete Recipe Action (only if the recipe belongs to the user)
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); 
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound(); // Return 404 if recipe not found or user doesn't own it
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        // View recipe details
        public async Task<IActionResult> Detail(int id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Fetch associated comments
            var comments = await _context.Comments
                .Where(c => c.RecipeId == id)
                .OrderByDescending(c => c.DateCreated) 
                .ToListAsync();

            // Calculate the average rating
            var ratings = await _context.Ratings.Where(r => r.RecipeId == id).ToListAsync();
            var averageRating = ratings.Any() ? ratings.Average(r => r.Value) : 0;

            // Get the logged-in user's rating and comment status
            int? userRating = null;
            bool hasCommented = false;

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check if the user has already rated the recipe
                var userRatingRecord = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId);
                userRating = userRatingRecord?.Value;

                // Check if the user has already commented on the recipe
                hasCommented = await _context.Comments
                    .AnyAsync(c => c.RecipeId == id && c.UserId == userId);
            }

            ViewData["AverageRating"] = averageRating;
            ViewData["UserRating"] = userRating;
            ViewData["Comments"] = comments; 
            ViewData["HasCommented"] = hasCommented; 

            return View(recipe);
        }




        // Action to show a random recipe's detail
        public async Task<IActionResult> WhatShouldICookToday()
        {
            var randomRecipe = await _context.Recipes
                .OrderBy(r => Guid.NewGuid())  // Randomly order the recipes
                .FirstOrDefaultAsync();

            if (randomRecipe == null)
            {
                return NotFound();  // Return 404 if no recipe is found
            }

            return RedirectToAction("Detail", new { id = randomRecipe.Id });
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> SearchByIngredients(string ingredient1, string ingredient2, string ingredient3, string ingredient4, string ingredient5, string ingredient6, string ingredient7)
        {
            // Store the ingredients to pre-fill the input fields
            ViewData["Ingredient1"] = ingredient1;
            ViewData["Ingredient2"] = ingredient2;
            ViewData["Ingredient3"] = ingredient3;
            ViewData["Ingredient4"] = ingredient4;
            ViewData["Ingredient5"] = ingredient5;
            ViewData["Ingredient6"] = ingredient6;
            ViewData["Ingredient7"] = ingredient7;

            // Gather all ingredients from the inputs
            var ingredientList = new List<string> { ingredient1, ingredient2, ingredient3, ingredient4, ingredient5, ingredient6, ingredient7 }
                .Where(i => !string.IsNullOrEmpty(i))  // Filter out empty inputs
                .Select(i => i.Trim().ToLower())      // Trim and convert to lowercase
                .ToList();

            // Start with all recipes
            var recipes = await _context.Recipes.ToListAsync(); // Fetch all recipes

            // If there are ingredients to filter by, filter recipes on the server side
            if (ingredientList.Any())
            {
                recipes = recipes
                    .Where(r => ingredientList.Any(ingredient =>
                        !string.IsNullOrEmpty(r.IngredientsString) && r.IngredientsString
                            .ToLower()                       // Convert IngredientsString to lowercase
                            .Split(',')                      
                            .Select(i => i.Trim())            // Trim each ingredient
                            .Any(ingredientInRecipe => ingredientInRecipe.Contains(ingredient))) // Check if the ingredient in the recipe contains the search term
                    )
                    .ToList();
            }

            // Calculate the average rating for each recipe
            foreach (var recipe in recipes)
            {
                var avgRating = _context.Ratings
                    .Where(r => r.RecipeId == recipe.Id)
                    .Average(r => (double?)r.Value); // Nullable to handle no ratings

                ViewData[$"AvgRating_{recipe.Id}"] = avgRating ?? 0; // If no ratings, set it to 0
            }

            // Return the filtered recipes (if any)
            return View(recipes);
        }



    }
}

