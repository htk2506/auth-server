using Destructurama.Attributed;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Api.V1.Dto.Users.Update
{
    public class UpdateUserResponseBody
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        [EmailAddress]
        [LogMasked]
        public string? Email { get; set; }

        public string Note { get; set; } = string.Empty;
    }
}
