namespace AuthServer.Dto.Users.Create
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; } 
        public string Username { get; set; } = null!;
    }
}
