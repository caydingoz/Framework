namespace Framework.AuthServer.Dtos.ActivityService.Output
{
    public class GetUserWorkItemOutput
    {
        public ICollection<GetUserWorkItem> WorkItems { get; set; } = [];
    }
    public class GetUserWorkItem
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
    }
}
