using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;

namespace Framework.AuthServer.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification, int>
    {
        Task<ICollection<NotificationDTO>> GetNotificationsForUserAsync(Guid userId, int page, int count);
        Task ReadNotificationAsync(int[] ids, Guid userId);
    }
}
