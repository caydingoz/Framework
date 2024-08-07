using Framework.Application;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Framework.Test.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Framework.AuthServer.Controllers
{
    [Route("api/[controller]")]
    public class SqlWithManyTestController : CRUDController<SqlWithManyTestModel, int>
    {
        public SqlWithManyTestController(IGenericRepository<SqlWithManyTestModel, int> repository) : base(repository)
        {
        }

        [HttpPost("included")]
        public async virtual Task<GeneralResponse<object>> TestIncludeAsync()
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var x = await Repository.FirstOrDefaultAsync(includes: x => x.SqlWithManyTestRelationModels);
                return x.SqlWithManyTestRelationModels is not null;
            });
        }
        [HttpPost("not-included")]
        public async virtual Task<GeneralResponse<object>> TestNotIncludeAsync()
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var x = await Repository.FirstOrDefaultAsync();
                return x.SqlWithManyTestRelationModels is null;
            });
        }
    }
}