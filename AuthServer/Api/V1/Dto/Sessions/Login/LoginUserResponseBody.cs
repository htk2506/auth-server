using Destructurama.Attributed;

namespace AuthServer.Api.V1.Dto.Sessions.Login
{
    public class LoginUserResponseBody
    {
        [LogMasked]
        public string SessionToken { get; set; } = null!;
    }
}
