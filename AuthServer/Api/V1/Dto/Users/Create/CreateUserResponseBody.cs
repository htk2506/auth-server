namespace AuthServer.Api.V1.Dto.Users.Create
{
    public class CreateUserResponseBody
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
    }
}
