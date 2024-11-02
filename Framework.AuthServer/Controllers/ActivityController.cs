using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.ActivityService.Input;
using Framework.AuthServer.Dtos.ActivityService.Output;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Consts;
using Framework.Shared.Dtos;
using Framework.Shared.Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ActivityController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<ActivityController> Logger;
        private readonly IMapper Mapper;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IGenericRepository<Activity, int> ActivityRepository;
        private readonly IGenericRepository<WorkItem, int> WorkItemRepository;

        public ActivityController(
            Configuration configuration,
            ILogger<ActivityController> logger,
            IMapper mapper,
            IGenericRepository<User, Guid> userRepository,
            IGenericRepository<Activity, int> activityRepository,
            IGenericRepository<WorkItem, int> workItemRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            UserRepository = userRepository;
            ActivityRepository = activityRepository;
            WorkItemRepository = workItemRepository;
        }

        [HttpGet]
        [Authorize(Policy = OperationNames.Activity + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetActivityForCalendarOutput>> GetActivityForCalendarAsync([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                Guid userId = GetUserIdGuid();

                var activities = await ActivityRepository.WhereAsync(x => x.UserId == userId && x.StartTime >= startTime && x.EndTime <= endTime, includes: x => x.WorkItem, readOnly: true);

                var res = new GetActivityForCalendarOutput { Activities = new List<ActivityDTO>() };

                var groupedByDay = activities.GroupBy(a => a.StartTime.Date).ToList();

                foreach (var group in groupedByDay)
                {
                    var dailyActivities = group.OrderBy(a => a.StartTime).ToList();

                    foreach (var activity in dailyActivities)
                    {
                        int layer = 0;
                        var activityDto = Mapper.Map<ActivityDTO>(activity);

                        foreach (var previousActivity in dailyActivities)
                        {
                            if (previousActivity.Id == activity.Id) break;

                            if (previousActivity.StartTime <= activity.StartTime && previousActivity.EndTime > activity.StartTime)
                                layer++;
                        }

                        activityDto.Layer = layer;
                        res.Activities.Add(activityDto);
                    }
                }

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.Activity + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateActivityAsync(CreateActivityInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                if (input.StartTime >= input.EndTime)
                    throw new Exception("Start date must be greater than the end");

                if (!await UserRepository.AnyAsync(x => x.Id == userId && x.WorkItems.Any(y => y.Id == input.WorkItemId)))
                    throw new Exception("There are no work items assigned to you that match the specified id.");

                List<Activity> activities = [];

                int loggedDay = input.EndTime.Day - input.StartTime.Day;

                for (int i = 0; i <= loggedDay; i++)
                {
                    var activity = Mapper.Map<Activity>(input);

                    activity.UserId = userId;
                    activity.StartTime = input.StartTime.AddDays(i);
                    activity.EndTime = input.EndTime.AddDays(i-loggedDay);

                    activities.Add(activity);
                }

                await ActivityRepository.InsertManyAsync(activities);

                return true;
            });
        }

        [HttpPut]
        [Authorize(Policy = OperationNames.Activity + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateActivityAsync(UpdateActivityInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                Guid userId = GetUserIdGuid();
                if (input.StartTime >= input.EndTime)
                    throw new Exception("Start date must be greater than the end");

                if (!await UserRepository.AnyAsync(x => x.WorkItems.Any(y => y.Id == input.WorkItemId)))
                    throw new Exception("There are no work items assigned to you that match the specified id.");

                var activity = await ActivityRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.UserId == userId) ?? throw new Exception("Activity not found.");

                if (!await UserRepository.AnyAsync(x => x.Id == activity.UserId && x.WorkItems.Any(y => y.Id == input.WorkItemId)))
                    throw new Exception("There are no work items assigned to you that match the specified id.");

                Mapper.Map(input, activity);

                await ActivityRepository.UpdateOneAsync(activity);

                return true;
            });
        }

        [HttpDelete]
        [Authorize(Policy = OperationNames.Activity + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteActivityAsync([FromQuery] int id)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                Guid userId = GetUserIdGuid();
                if(!await ActivityRepository.AnyAsync(x => x.Id == id && x.UserId == userId))
                    throw new Exception("Activity not found.");

                await ActivityRepository.DeleteOneAsync(id);

                return true;
            });
        }

        [HttpPost("workItem")]
        [Authorize(Policy = OperationNames.WorkItem + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateWorkItemAsync([FromQuery] string title)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await WorkItemRepository.AnyAsync(x => x.Title == title))
                    throw new Exception("There is already a work item with given title.");

                var workItem = new WorkItem { Title = title };

                await WorkItemRepository.InsertOneAsync(workItem);

                return true;
            });
        }

        [HttpDelete("workItem")]
        [Authorize(Policy = OperationNames.WorkItem + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteWorkItemAsync([FromQuery] int id)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await WorkItemRepository.DeleteOneAsync(id);

                return true;
            });
        }

        [HttpGet("workItem/user")]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetUserWorkItemOutput>> GetUserWorkItemAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();

                var user = await UserRepository.GetByIdAsync(userId, includes: x => x.WorkItems, readOnly: true) ?? throw new Exception("There is no user with token id.");

                var res = new GetUserWorkItemOutput();

                foreach (var workItem in user.WorkItems)
                    res.WorkItems.Add(new GetUserWorkItem { Id = workItem.Id, Title = workItem.Title });

                return res;
            });
        }

        [HttpPost("workItem/user")]
        [Authorize(Policy = OperationNames.WorkItem + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> AddWorkItemToUserAsync([FromQuery] Guid userId, [FromQuery] int workItemId)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var workItem = await WorkItemRepository.GetByIdAsync(workItemId) ?? throw new Exception("There is no work item with given id.");

                var user = await UserRepository.GetByIdAsync(userId) ?? throw new Exception("There is no user with given id.");

                workItem.Users.Add(user);

                await WorkItemRepository.UpdateOneAsync(workItem);

                return true;
            });
        }

        [HttpDelete("workItem/user")]
        [Authorize(Policy = OperationNames.WorkItem + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteWorkItemFromUserAsync([FromQuery] Guid userId, [FromQuery] int workItemId)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var workItem = await WorkItemRepository.GetByIdAsync(workItemId) ?? throw new Exception("There is no work item with given id.");

                var user = await UserRepository.GetByIdAsync(userId) ?? throw new Exception("There is no user with given id.");

                workItem.Users.Remove(user);

                await WorkItemRepository.UpdateOneAsync(workItem);

                return true;
            });
        }
    }
}