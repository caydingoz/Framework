using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.AbsenceService.Input;
using Framework.AuthServer.Dtos.AbsenceService.Output;
using Framework.AuthServer.Enums;
using Framework.AuthServer.Helpers;
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
    public class AbsenceController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<AbsenceController> Logger;
        private readonly IMapper Mapper;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IGenericRepository<Absence, int> AbsenceRepository;
        private readonly INotificationRepository NotificationRepository;
        private readonly IHubContext<NotificationHub> HubContext;

        public AbsenceController(
            Configuration configuration,
            ILogger<AbsenceController> logger,
            IMapper mapper,
            IGenericRepository<User, Guid> userRepository,
            IGenericRepository<Absence, int> absenceRepository,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            UserRepository = userRepository;
            AbsenceRepository = absenceRepository;
            NotificationRepository = notificationRepository;
            HubContext = hubContext;
        }

        [HttpGet("admin/requests")]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetAllAbsenceRequestsOutput>> GetAllAbsenceRequestsAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] AbsenceTypes? type, [FromQuery] string? filterName, [FromQuery] AbsenceStatus? status)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserId();

                var absences = await AbsenceRepository.WhereAsync(x => 
                    (type == null || x.Type == type) &&
                    (status == null || x.Status == status) &&
                    (filterName == null || x.User.FirstName.Contains(filterName) || x.User.LastName.Contains(filterName) || x.User.Email.Contains(filterName) || x.User.PhoneNumber.Contains(filterName))
                    , includes: x => x.User, pagination: new Pagination { Page = page, Count = count });

                var res = new GetAllAbsenceRequestsOutput();

                foreach (var absence in absences)
                {
                    if (absence.User == null)
                        throw new Exception("User not found.");

                    var absenceDto = Mapper.Map<GetAllAbsenceRequests>(absence);
                    absenceDto.FirstName = absence.User.FirstName;
                    absenceDto.LastName = absence.User.LastName;
                    absenceDto.Email = absence.User.Email;
                    absenceDto.PhoneNumber = absence.User.PhoneNumber;
                    absenceDto.Image = absence.User.Image;

                    res.Absences.Add(absenceDto);
                }

                res.TotalCount = await AbsenceRepository.CountAsync(x =>
                    x.Status == status &&
                    (type == null || x.Type == type) &&
                    (filterName == null || x.User.FirstName.Contains(filterName) || x.User.LastName.Contains(filterName) || x.User.Email.Contains(filterName) || x.User.PhoneNumber.Contains(filterName))
                    );

                return res;
            });
        }

        [HttpGet("user/requests")]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetUserAbsenceRequestsOutput>> GetUserAbsenceRequestsAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] AbsenceTypes? type, [FromQuery] string? description, [FromQuery] AbsenceStatus? status)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var user = await UserRepository.GetByIdAsync(userId);

                var absences = await AbsenceRepository.WhereAsync(x => 
                    x.UserId == userId &&
                    (status == null || x.Status == status) &&
                    (type == null || x.Type == type) &&
                    (description == null || x.Description.Contains(description))
                    , pagination: new Pagination { Page = page, Count = count }, sorts: [new Sort { Name = nameof(Absence.CreatedAt), Type = SortTypes.DESC }]);

                var res = new GetUserAbsenceRequestsOutput();

                foreach (var absence in absences)
                {
                    var absenceDto = Mapper.Map<AbsenceDTO>(absence);

                    res.Absences.Add(absenceDto);
                }

                res.TotalCount = await AbsenceRepository.CountAsync(x =>
                    x.UserId == userId &&
                    (status == null || x.Status == status) &&
                    (type == null || x.Type == type) &&
                    (description == null || x.Description.Contains(description)));

                res.EmploymentDate = user.EmploymentDate;

                return res;
            });
        }

        [HttpGet("user/accurals")]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetUserAbsenceInfoOutput>> GetUserAbsenceInfoAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var user = await UserRepository.GetByIdAsync(userId, includes: x => x.Absences) ?? throw new Exception("User not found.");

                var res = new GetUserAbsenceInfoOutput();

                int workYears = DateTime.Now.Year - user.EmploymentDate.Year;

                for (int i = 0; i <= workYears; i++)
                {
                    int annualDate = i == 0 ? 0 : 14;
                    double usedDay = user.Absences.Where(x => x.StartTime >= user.EmploymentDate.AddYears(i)
                        && x.StartTime <= user.EmploymentDate.AddYears(i + 1)
                        && x.Status == AbsenceStatus.Approved).Sum(x => x.Duration);

                    res.AbsenceInfos.Add(new AbsenceInfo
                    {
                        AnnualDay = annualDate,
                        AnnualStart = user.EmploymentDate.AddYears(i),
                        AnnualEnd = user.EmploymentDate.AddYears(i + 1).AddDays(-1),
                        RemainingDay = annualDate - usedDay,
                        UsedDay = usedDay
                    });
                }

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateAbsenceRequestAsync(CreateAbsenceRequestInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                var user = await UserRepository.GetByIdAsync(userId) ?? throw new Exception("User not found.");

                if (input.StartTime.DayOfWeek == DayOfWeek.Saturday || input.StartTime.DayOfWeek == DayOfWeek.Sunday)
                    throw new Exception("Start time cannot be on a weekend.");

                if (input.StartTime >= input.EndTime)
                    throw new Exception("Start time must be earlier than the end time.");

                LeaveCalculator.ValidateAbsenceDate(input.StartTime, user.EmploymentDate);
                LeaveCalculator.ValidateAbsenceDate(input.EndTime, user.EmploymentDate);

                var duration = LeaveCalculator.CalculateBusinessDaysDuration(input.StartTime, input.EndTime);

                if(user.TotalAbsenceEntitlement < duration)
                    throw new Exception($"Your leave balance is insufficient. Current leave balance: {user.TotalAbsenceEntitlement} days.");

                var absence = Mapper.Map<Absence>(input);
                absence.UserId = userId;
                absence.Duration = duration;

                await AbsenceRepository.InsertOneAsync(absence);

                return true;
            });
        }

        [HttpPut]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> UpdateAbsenceRequestStatusAsync(UpdateAbsenceRequestStatusInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                Guid userId = GetUserIdGuid();

                var absence = await AbsenceRepository.GetByIdAsync(input.Id) ?? throw new Exception("Absence not found.");

                if(absence.Status != AbsenceStatus.Pending)
                    throw new Exception("You can only approve/reject leave requests that are in pending status.");

                var absenceUser = await UserRepository.GetByIdAsync(absence.UserId) ?? throw new Exception("User not found.");

                if (input.Status == AbsenceStatus.Approved && absenceUser.TotalAbsenceEntitlement < absence.Duration)
                    throw new Exception($"User leave balance is insufficient. Current leave balance: {absenceUser.TotalAbsenceEntitlement} days.");

                absence.Status = input.Status;

                await AbsenceRepository.UpdateOneAsync(absence);

                if (input.Status == AbsenceStatus.Approved)
                    absenceUser.TotalAbsenceEntitlement -= absence.Duration;

                await UserRepository.UpdateOneAsync(absenceUser);

                var notification = new Notification 
                {
                    Message = $"Your leave request has been {(input.Status == AbsenceStatus.Approved ? "approved" : "rejected")}.",
                    Title = "Leave Requests",
                    Url = "/absence-management/requests/user",
                    Type = NotificationTypes.Info,
                    NotificationUsers = [],
                };

                var date = DateTime.UtcNow;
                notification.NotificationUsers.Add(new NotificationUser
                {
                    UserId = absence.UserId,
                    CreatedAt = date,
                    UpdatedAt = date
                });

                await NotificationRepository.InsertOneAsync(notification);
                await HubContext.Clients.User(absence.UserId.ToString()).SendAsync("ReceiveNotification", 1);

                return true;
            });
        }

        [HttpDelete("user")]
        [Authorize(Policy = OperationNames.Absence + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteAbsenceRequestAsync([FromQuery] int id)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                Guid userId = GetUserIdGuid();
                
                var absence = await AbsenceRepository.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId) ?? throw new Exception("Absence not found.");

                if (absence.Status != AbsenceStatus.Pending)
                    throw new Exception("You can only delete leave requests that are in pending status.");

                await AbsenceRepository.DeleteOneAsync(id);

                return true;
            });
        }
    }
}