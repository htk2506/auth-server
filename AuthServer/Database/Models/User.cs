using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Database.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-z\d]+$")]
        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
