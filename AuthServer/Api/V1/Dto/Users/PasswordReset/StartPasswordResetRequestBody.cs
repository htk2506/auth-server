using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.PasswordReset
{
    public class StartPasswordResetRequestBody
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = null!;
    }
}
