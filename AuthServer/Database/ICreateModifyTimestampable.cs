namespace AuthServer.Database
{
    public interface ICreateModifyTimestampable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
