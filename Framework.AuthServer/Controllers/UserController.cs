using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Consts;
using Framework.AuthServer.Dtos.UserService.Input;
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
using System.Text.RegularExpressions;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<UserController> Logger;
        private readonly IMapper Mapper;
        private readonly IGenericRepository<User, Guid> UserRepository;
        private readonly IGenericRepository<Role, int> RoleRepository;

        public UserController(
            Configuration configuration,
            ILogger<UserController> logger,
            IMapper mapper,
            IGenericRepository<User, Guid> userRepository,
            IGenericRepository<Role, int> roleRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            UserRepository = userRepository;
            RoleRepository = roleRepository;
        }

        [HttpGet]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.ReadAccess)]
        public async Task<GeneralResponse<GetUsersOutput>> GetUsersAsync([FromQuery] int page, [FromQuery] int count, [FromQuery] UserStatusEnum? status, [FromQuery] SortTypes? sortType, [FromQuery] string? column, [FromQuery] string? filterName)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var sort = new Sort { Name = column ?? "Id", Type = sortType ?? SortTypes.ASC };
                var pagination = new Pagination { Page = page, Count = count };

                var users = await UserRepository.WhereAsync(x => 
                                                    (filterName == null || x.FirstName.Contains(filterName) || x.LastName.Contains(filterName) || x.Email.Contains(filterName))
                                                    && (status == null || x.Status == status)
                                                    , includes: x => x.Roles, readOnly: true, pagination: pagination, sorts: [sort]);

                var res = new GetUsersOutput();

                foreach (var user in users)
                    res.Users.Add(new UserOutput 
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Title = user.Title,
                        Status = user.Status,
                        Image = user.Image,
                        Roles = user.Roles.Select(x => new UserRoleOutput { Id = x.Id, Name = x.Name }),
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });

                res.TotalCount = await UserRepository.CountAsync(x => (filterName == null ||
                                                    x.FirstName.Contains(filterName) ||
                                                    x.LastName.Contains(filterName) ||
                                                    x.Email.Contains(filterName)) &&
                                                    (status == null || x.Status == status));

                return res;
            });
        }

        [HttpPost]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> CreateUserAsync(CreateUserInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if (await UserRepository.AnyAsync(x => x.Email == input.Email))
                    throw new Exception("There is already exist a user with given email!");

                input.PhoneNumber = FormatPhoneNumber(input.PhoneNumber);

                if (!IsPhoneNumberValid(input.PhoneNumber))
                    throw new Exception("Phone number is not valid!");

                var user = Mapper.Map<User>(input);

                user.Status = UserStatusEnum.Active;
                user.Password = input.FirstName + "123$";

                var roles = await RoleRepository.WhereAsync(x => input.RoleIds.Contains(x.Id));

                if (roles.Count == 0)
                    throw new Exception("There is no role with given ids!");

                user.Roles = roles;

                await UserRepository.InsertOneAsync(user);

                return true;
            });
        }

        [HttpPut]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.WriteAccess)]
        public async Task<GeneralResponse<object>> UpdateUserAsync(UpdateUserInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                if(await UserRepository.AnyAsync(x => x.Email == input.Email && x.Id != input.Id))
                    throw new Exception("There is already exist a user with given email!");

                var user = await UserRepository.GetByIdAsync(input.Id, includes: x => x.Roles) ?? throw new Exception("User not found!");

                input.PhoneNumber = FormatPhoneNumber(input.PhoneNumber);

                if (!IsPhoneNumberValid(input.PhoneNumber))
                    throw new Exception("Phone number is not valid!");

                Mapper.Map(input, user);

                var roles = await RoleRepository.WhereAsync(x => input.RoleIds.Contains(x.Id));

                if (roles.Count == 0)
                    throw new Exception("There is no role with given ids!");

                user.Roles = roles;

                await UserRepository.UpdateOneAsync(user);

                return true;
            });
        }

        [HttpDelete]
        [Authorize(Policy = OperationNames.User + PermissionAccessTypes.DeleteAccess)]
        public async Task<GeneralResponse<object>> DeleteUserAsync(DeleteUserInput input)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                input.Ids = input.Ids.Distinct().ToList();

                if (await UserRepository.AnyAsync(x => input.Ids.Contains(x.Id) && x.FirstName == "ADMINISTRATOR"))
                    throw new Exception("Changes to the admin are not allowed!");

                var users = await UserRepository.WhereAsync(x => input.Ids.Contains(x.Id));

                if (users.Count == 0) throw new Exception("There is no user with given ids.");
                
                if (users.Any(x => x.Status == UserStatusEnum.Deleted)) throw new Exception("The selected user(s) have already been deleted.");

                foreach (var user in users)
                {
                    user.IsDeleted = true;
                    user.Status = UserStatusEnum.Deleted;
                }

                await UserRepository.UpdateManyAsync(users);

                return true;
            });
        }
        private static string FormatPhoneNumber(string phoneNumber)
        {
            phoneNumber = Regex.Replace(phoneNumber, @"\D", "");

            if (phoneNumber.StartsWith('0'))
                phoneNumber = phoneNumber[1..];

            if (phoneNumber.Length != 10)
                throw new Exception("The phone number must consist of 10 digits and only numbers are allowed.");

            return string.Format("({0}) {1} {2} {3}",
                phoneNumber.Substring(0, 3),
                phoneNumber.Substring(3, 3), 
                phoneNumber.Substring(6, 2), 
                phoneNumber.Substring(8, 2));
        }
        private static bool IsPhoneNumberValid(string phoneNumber)
        {
            string pattern = @"^\(5\d{2}\) \d{3} \d{2} \d{2}$";
            Regex regex = new(pattern);

            return regex.IsMatch(phoneNumber);
        }
    }
}