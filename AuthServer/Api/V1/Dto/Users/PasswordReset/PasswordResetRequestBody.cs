using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.PasswordReset
{
    public class PasswordResetRequestBody
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordResetToken { get; set; } = null!;
      
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
