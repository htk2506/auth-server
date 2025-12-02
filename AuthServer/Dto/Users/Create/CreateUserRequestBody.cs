using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto.Users.Create
{
    public class CreateUserRequestBody
    {
        [Required]
        [RegularExpression(@"^[a-z\d]{1,254}$")]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? Note { get; set; }
    }
}
