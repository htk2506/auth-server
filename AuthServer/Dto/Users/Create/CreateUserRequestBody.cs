using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto.Users.Create
{
    public class CreateUserRequestBody
    {
        [Required]
        [RegularExpression(@"^[a-z\d]{1,254}$")]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? Note { get; set; }
    }
}
