using Destructurama.Attributed;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.Update
{
    public class UpdateUserPasswordRequestBody
    {
        [Required]
        [LogMasked]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [LogMasked]
        public string NewPassword { get; set; } = null!;
    }
}
