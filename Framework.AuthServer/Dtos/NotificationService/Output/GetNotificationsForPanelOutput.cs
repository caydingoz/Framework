namespace Framework.AuthServer.Dtos.NotificationService.Output
{
    public class GetNotificationsForPanelOutput
    {
        public ICollection<NotificationForPanelDTO> Notifications { get; set; } = [];
        public long TotalCount { get; set; }
    }
}
