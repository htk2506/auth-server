using System.ComponentModel.DataAnnotations;

namespace AuthServer.Database.Models
{
    public class PasswordResetToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string TokenHash { get; set; } = null!;

        [Required]
        public AppUser AppUser { get; set; } = null!;

        [Required]
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
