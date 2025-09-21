using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.EF;
using Microsoft.EntityFrameworkCore;

namespace Framework.AuthServer.Repositories
{
    public class NotificationRepository : EfCoreRepositoryBase<Notification, AuthServerDbContext, int>, INotificationRepository
    {
        public NotificationRepository(AuthServerDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ICollection<NotificationDTO>> GetNotificationsForUserAsync(Guid userId, int page, int count)
        {
            return await DbContext.Notifications
                .Where(n => n.NotificationUsers.Any(nu => nu.UserId == userId) && !n.IsDeleted)
                .Select(n => new
                {
                    Notification = n,
                    UserNotification = n.NotificationUsers.FirstOrDefault(nu => nu.UserId == userId)
                })
                .Select(n => new NotificationDTO
                {
                    Id = n.Notification.Id,
                    Message = n.Notification.Message,
                    Title = n.Notification.Title,
                    CreatedAt = n.Notification.CreatedAt,
                    Type = n.Notification.Type,
                    IsRead = n.UserNotification != null && n.UserNotification.IsRead,
                    Url = n.Notification.Url
                })
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .Skip(page * count)
                .Take(count)
                .ToListAsync();
        }

        public async Task ReadNotificationAsync(int[] ids, Guid userId)
        {
            await DbContext.NotificationUsers.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRead, true));
        }
    }
}
