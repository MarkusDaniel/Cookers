using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookMaster.Models
{
    [Table("Recipes")]
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ImageUrl { get; set; }  // Store the image URL or path

        public string? UserId { get; set; } // Nullable to support recipes without a user

        // Ingredients as a comma-separated string
        public string IngredientsString { get; set; }

        [NotMapped]
        public List<string> Ingredients
        {
            get => IngredientsString?.Split(',').ToList() ?? new List<string>();
            set => IngredientsString = string.Join(",", value);
        }
    }
}
