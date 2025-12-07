using System.ComponentModel.DataAnnotations;

namespace AuthServer.Database.Models
{
    public class UserSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public AppUser AppUser { get; set; } = null!;

        [Required]
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
