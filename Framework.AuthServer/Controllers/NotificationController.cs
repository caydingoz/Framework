using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.NotificationService.Input;
using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Hubs;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Entities;
using Framework.Shared.Entities.Configurations;
using Framework.Shared.Enums;
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
        private readonly IGenericRepository<Notification, int> NotificationRepository;

        public NotificationController(
            Configuration configuration,
            ILogger<NotificationController> logger,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            IGenericRepository<Notification, int> notificationRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            HubContext = hubContext;
            NotificationRepository = notificationRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<GeneralResponse<GetNotificationsOutput>> GetNotificationsAsync([FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var sortList = new List<Sort>
                {
                    new() { Name = nameof(Notification.IsRead), Type = SortTypes.ASC },
                    new() { Name = nameof(Notification.CreatedAt), Type = SortTypes.DESC }
                };

                var pagination = new Pagination { Page = page, Count = count };

                var notifications = await NotificationRepository.WhereAsync(x => x.UserId == userId, readOnly: true, pagination: pagination, sorts: sortList);

                var res = new GetNotificationsOutput
                {
                    Notifications = Mapper.Map<ICollection<NotificationDTO>>(notifications),

                    TotalCount = await NotificationRepository.CountAsync(x => x.UserId == userId)
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
                var notifications = Mapper.Map<ICollection<Notification>>(input.Notifications);

                await NotificationRepository.InsertManyAsync(notifications);

                var userIds = input.Notifications.Select(x => x.UserId);

                var notificationTasks = userIds.Select(userId => HubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", 1));

                await Task.WhenAll(notificationTasks);

                return true;
            });
        }

        [HttpPut("read")]
        [Authorize]
        public async Task<GeneralResponse<object>> ReadNotificationAsync([FromQuery] int id)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                var notification = await NotificationRepository.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId) ?? throw new Exception("Notification not found! ID: " + id);

                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;

                await NotificationRepository.UpdateOneAsync(notification);

                return true;
            });
        }
    }
}