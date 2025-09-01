namespace AuthServer.Dto
{
    public class RegisterUserRequest
    {
        public required string Username { get; set; }

        public required string Password { get; set; }

        public string? Notes { get; set; }
    }
}
