using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.PasswordReset
{
    public class PasswordResetRequestBody
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string PasswordResetToken { get; set; } = null!;
      
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
