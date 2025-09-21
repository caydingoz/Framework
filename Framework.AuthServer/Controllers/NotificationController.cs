using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.NotificationService.Input;
using Framework.AuthServer.Dtos.NotificationService.Output;
using Framework.AuthServer.Enums;
using Framework.AuthServer.Hubs;
using Framework.AuthServer.Interfaces.Repositories;
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
        private readonly INotificationRepository NotificationRepository;
        private readonly IGenericRepository<NotificationUser, int> NotificationUserRepository;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IGenericRepository<Role, int> RoleRepository;

        public NotificationController(
            Configuration configuration,
            ILogger<NotificationController> logger,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            INotificationRepository notificationRepository,
            IGenericRepository<NotificationUser, int> notificationUserRepository,
            IGenericRepository<User, Guid> userRepository,
            IGenericRepository<Role, int> roleRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            HubContext = hubContext;
            NotificationRepository = notificationRepository;
            NotificationUserRepository = notificationUserRepository;
            UserRepository = userRepository;
            RoleRepository = roleRepository;
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

        [HttpPut("user/read")]
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

        [HttpGet]
        [Authorize]
        [Authorize(Policy = OperationNames.NotificationAdmin + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetNotificationsForPanelOutput>> GetNotificationsForPanelAsync([FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var res = new GetNotificationsForPanelOutput();

                var userId = GetUserIdGuid();
                var sort = new Sort { Name = "CreatedAt", Type = SortTypes.DESC };
                var pagination = new Pagination { Page = page, Count = count };

                var notifications = await NotificationRepository.WhereAsync(x => x.Type == NotificationTypes.Admin, includes: x => x.NotificationRoles.Select(y => y.Role), readOnly: true, pagination: pagination, sorts: [sort], includeLogicalDeleted: true);

                foreach (var notification in notifications)
                {
                    res.Notifications.Add(new NotificationForPanelDTO
                    {
                        Id = notification.Id,
                        Title = notification.Title,
                        Message = notification.Message,
                        CreatedAt = notification.CreatedAt,
                        IsDeleted = notification.IsDeleted,
                        Roles = notification.NotificationRoles.ToDictionary(x => x.RoleId, x => x.Role.Name)
                    });
                }

                res.TotalCount = await NotificationRepository.CountAsync(x => x.Type == NotificationTypes.Admin, includeLogicalDeleted: true);

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.NotificationAdmin + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateNotificationsAsync(CreateNotificationInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                input.RoleIds = new HashSet<int>(input.RoleIds).ToList();

                if (input.RoleIds.Count == 0)
                    throw new Exception("At least one role must be selected!");

                //TODO: Roles check
                var notification = Mapper.Map<Notification>(input);
                notification.NotificationUsers = [];
                notification.NotificationRoles = input.RoleIds.Select(roleId => new NotificationRole
                {
                    RoleId = roleId
                }).ToList();

                var userIds = await UserRepository.WhereWithSelectAsync(x => x.Roles.Any(role => input.RoleIds.Contains(role.Id)), selector: x => x.Id, readOnly: true, includes: x => x.Roles);

                foreach (var userId in userIds)
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

                var notificationTasks = userIds.Select(userId => HubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", 1));

                await Task.WhenAll(notificationTasks);

                return true;
            });
        }

        [HttpDelete]
        [Authorize]
        [Authorize(Policy = OperationNames.NotificationAdmin + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteNotificationsForPanelAsync(int[] ids)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                var notifications = await NotificationRepository.WhereAsync(x => ids.Contains(x.Id));

                foreach (var notification in notifications)
                {
                    notification.IsDeleted = true;
                }

                if (notifications.Count != 0)
                    await NotificationRepository.UpdateManyAsync(notifications);

                return true;
            });
        }
    }
}