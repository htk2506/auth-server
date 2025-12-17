using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dto.Users.Update
{
    public class UpdateUserRequestBody
    {
        [Required]
        [MinLength(3)]
        [MaxLength(32)]
        [RegularExpression(@"^[A-Za-z0-9](_?[A-Za-z0-9])*$")]
        public string Username { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
    }
}
