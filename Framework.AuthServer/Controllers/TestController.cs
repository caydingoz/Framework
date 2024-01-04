using Framework.Application;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class TestController : CRUDController<UserRefreshToken, int>
    {
        public TestController(IUserRefreshTokenRepository repository) : base(repository)
        {
        }
    }
}