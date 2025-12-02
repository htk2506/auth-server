namespace AuthServer.Dto.Sessions.Login
{
    public class LoginUserRequestBody
    {
        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}
