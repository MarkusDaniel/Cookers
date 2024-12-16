using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cookers.Models
{

    namespace CookMaster.Models
    {
        public class Favorite
        {
            public int Id { get; set; }
            public string? UserId { get; set; }
            public int? RecipeId { get; set; } 
        }
    }
}
