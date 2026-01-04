using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.Update
{
    public class UpdateUserPasswordRequestBody
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
