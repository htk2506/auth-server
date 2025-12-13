namespace AuthServer.Dto.Users.Get
{
    public class GetUserResponseBody
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
    }
}
