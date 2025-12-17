namespace AuthServer.Dto.Users.Update
{
    public class UpdateUserResponseBody
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
    }
}
