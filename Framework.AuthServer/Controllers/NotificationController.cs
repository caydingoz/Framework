using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.NotificationService.Input;
using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Hubs;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class NotificationController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<NotificationController> Logger;
        private readonly IMapper Mapper;
        private readonly IHubContext<NotificationHub> HubContext;
        private readonly INotificationRepository NotificationRepository;
        private readonly IGenericRepository<NotificationUser, int> NotificationUserRepository;

        public NotificationController(
            Configuration configuration,
            ILogger<NotificationController> logger,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            INotificationRepository notificationRepository,
            IGenericRepository<NotificationUser, int> notificationUserRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            HubContext = hubContext;
            NotificationRepository = notificationRepository;
            NotificationUserRepository = notificationUserRepository;
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<GeneralResponse<GetNotificationsOutput>> GetNotificationsForUserAsync([FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var notifications = await NotificationRepository.GetNotificationsForUserAsync(userId, page, count);

                var res = new GetNotificationsOutput
                {
                    Notifications = notifications,
                    UnreadCount = await NotificationUserRepository.CountAsync(x => x.UserId == userId && x.IsRead == false),
                    TotalCount = await NotificationUserRepository.CountAsync(x => x.UserId == userId)
                };

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.NotificationAdmin + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateNotificationsAsync(CreateNotificationInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                input.UserIds = new HashSet<Guid>(input.UserIds).ToList();

                var notification = Mapper.Map<Notification>(input);
                notification.NotificationUsers = [];

                foreach (var userId in input.UserIds)
                {
                    var date = DateTime.UtcNow;
                    notification.NotificationUsers.Add(new NotificationUser
                    {
                        UserId = userId,
                        CreatedAt = date,
                        UpdatedAt = date
                    });
                }

                await NotificationRepository.InsertOneAsync(notification);

                var notificationTasks = input.UserIds.Select(userId => HubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", 1));

                await Task.WhenAll(notificationTasks);

                return true;
            });
        }

        [HttpPut("read")]
        [Authorize]
        public async Task<GeneralResponse<object>> ReadNotificationAsync([FromQuery] int[] ids)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var idList = new HashSet<int>(ids).ToList();

                var userId = GetUserIdGuid();

                var notificationUsers = await NotificationUserRepository.WhereAsync(x => idList.Contains(x.NotificationId) && x.UserId == userId);

                foreach (var notificationUser in notificationUsers)
                {
                    notificationUser.IsRead = true;
                }

                await NotificationUserRepository.UpdateManyAsync(notificationUsers);

                await HubContext.Clients.User(userId.ToString()).SendAsync("ReadNotification", notificationUsers.Select(x => x.NotificationId));

                return true;
            });
        }
    }
}