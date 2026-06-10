using Destructurama.Attributed;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.PasswordReset
{
    public class PasswordResetRequestBody
    {
        [EmailAddress]
        [Required]
        [LogMasked]
        public string Email { get; set; } = null!;

        [Required]
        [LogMasked]
        public string PasswordResetToken { get; set; } = null!;
      
        [Required]
        [LogMasked]
        public string NewPassword { get; set; } = null!;
    }
}
