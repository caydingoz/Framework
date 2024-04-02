using Framework.Application;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Framework.Test.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class SqlWithOneTestController : CRUDController<SqlWithOneTestModel, int>
    {
        public SqlWithOneTestController(IGenericRepository<SqlWithOneTestModel, int> repository) : base(repository)
        {
        }

        [HttpPost("included")]
        public async virtual Task<GeneralResponse<object>> TestIncludeAsync()
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var x = await Repository.WhereAsync(x => 1 == 1, includes: x => x.SqlWithOneTestRelationModels);
                return true;
                //return x.SqlWithManyTestRelationModels is not null;
            });
        }
        [HttpPost("not-included")]
        public async virtual Task<GeneralResponse<object>> TestNotIncludeAsync()
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var x = await Repository.FirstOrDefaultAsync();
                return x.SqlWithOneTestRelationModels is null;
            });
        }
    }
}