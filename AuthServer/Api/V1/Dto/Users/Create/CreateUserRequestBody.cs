using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.Create
{
    public class CreateUserRequestBody
    {
        [Required]
        [MinLength(3)]
        [MaxLength(32)]
        [RegularExpression(@"^[A-Za-z0-9](_?[A-Za-z0-9])*$")]
        public string Username { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Password { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
    }
}
