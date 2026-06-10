using System.ComponentModel.DataAnnotations;
using Destructurama.Attributed;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Database.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class AppUser : ICreateModifyTimestampable, ISoftDeletable
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(32)]
        [RegularExpression(@"^[a-z0-9](_?[a-z0-9])*$")]
        public string Username { get; set; } = null!;

        [EmailAddress]
        [LogMasked]
        public string? Email { get; set; }

        [Required]
        [LogMasked]
        public string PasswordHash { get; set; } = null!;

        public string Note { get; set; } = string.Empty;

        // Interface implementations 
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTimeOffset? DeletedAt { get; set; }
    }
}
