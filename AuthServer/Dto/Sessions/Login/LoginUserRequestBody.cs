using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto.Sessions.Login
{
    public class LoginUserRequestBody
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
