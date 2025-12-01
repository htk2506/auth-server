namespace AuthServer.Dto.Users.Create
{
    public class CreateUserResponseBody
    {
        public Guid Id { get; set; } 
        public string Username { get; set; } = null!;
    }
}
