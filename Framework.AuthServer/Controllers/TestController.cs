using Framework.Application;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class TestController : CRUDController<UserRefreshToken, int>
    {
        public TestController(IGenericRepository<UserRefreshToken, int> repository) : base(repository)
        {
        }
    }
}