using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto
{
    public class LoginUserRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
