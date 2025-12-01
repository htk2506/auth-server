using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto.Users.Create
{
    public class CreateUserRequestBody
    {
        [RegularExpression(@"^[a-z\d]+$")]
        public required string Username { get; set; }

        [MinLength(1, ErrorMessage = "Missing password")]
        public required string Password { get; set; }

        public string? Notes { get; set; }
    }
}
