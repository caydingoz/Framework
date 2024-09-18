using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.RoleService.Input;
using Framework.AuthServer.Dtos.RoleService.Output;
using Framework.AuthServer.Dtos.UserService.Output;
using Framework.AuthServer.Enums;
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

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<UserController> Logger;

        private readonly IGenericRepository<User, Guid> UserRepository;

        public UserController(
            Configuration configuration,
            ILogger<UserController> logger,
            IGenericRepository<User, Guid> userRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            UserRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetUsersOutput>> GetRolesAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] string? column, [FromQuery] SortTypes sortType, [FromQuery] string? filterName)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType };
                var pagination = new Pagination { Page = page, Count = count };

                var users = await UserRepository.WhereAsync(x => filterName == null || 
                                                    x.FirstName.Contains(filterName) || 
                                                    x.LastName.Contains(filterName) ||
                                                    x.Email.Contains(filterName)
                                                    , includes: x => x.Roles, readOnly: true, pagination: pagination, sorts: [sort]);

                var res = new GetUsersOutput();

                foreach (var user in users)
                    res.Users.Add(new UserOutput 
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Title = user.Title,
                        Status = user.Status,
                        Image = user.Image,
                        Roles = user.Roles.Select(x => x.Name),
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });

                res.TotalCount = await UserRepository.CountAsync(x => filterName == null ||
                                                    x.FirstName.Contains(filterName) ||
                                                    x.LastName.Contains(filterName) ||
                                                    x.Email.Contains(filterName));

                return res;
            });
        }
    }
}