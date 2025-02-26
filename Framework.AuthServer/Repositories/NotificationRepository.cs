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
                .Where(n => n.NotificationUsers.Any(nu => nu.UserId == userId))
                .Select(n => new
                {
                    Notification = n,
                    UserNotification = n.NotificationUsers.FirstOrDefault(nu => nu.UserId == userId)
                })
                .Select(nu => new NotificationDTO
                {
                    Id = nu.Notification.Id,
                    Message = nu.Notification.Message,
                    Title = nu.Notification.Title,
                    CreatedAt = nu.Notification.CreatedAt,
                    Type = nu.Notification.Type,
                    IsRead = nu.UserNotification != null && nu.UserNotification.IsRead
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
