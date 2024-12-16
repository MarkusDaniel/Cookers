using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cookers.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserId { get; set; } // User who made the comment
        public int RecipeId { get; set; }  // Recipe this comment is associated with
        public string Content { get; set; } // Comment content
        public DateTime DateCreated { get; set; } // When the comment was created
    }

}
