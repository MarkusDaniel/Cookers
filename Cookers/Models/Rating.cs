using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookMaster.Models
{
    [Table("Ratings")]
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Value { get; set; }
    }
}
