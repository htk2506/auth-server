using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto
{
    public class RegisterUserRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? Notes { get; set; }
    }
}
