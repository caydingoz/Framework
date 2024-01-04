using Framework.Domain.Interfaces.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Application
{
    public class CRUDController<T,U> : BaseController where T : IBaseEntity<U> where U : struct
    {
        protected IGenericRepository<T,U> Repository { get; set; }
        public CRUDController(IGenericRepository<T, U> repository)
        {
            Repository = repository;
        }

        [HttpGet]
        public async virtual Task<GeneralResponse<ICollection<T>>> GetAllAsync()
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var res = await Repository.GetAllAsync(true);
                return res;
            });
        }
        [HttpGet("{id}")]
        public async virtual Task<GeneralResponse<T?>> GetByIdAsync([FromRoute] U id)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var res = await Repository.GetByIdAsync(id, true);
                return res;
            });
        }
        [HttpPost]
        public async virtual Task<GeneralResponse<object>> InsertOneAsync(T entity)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                SetIdToDefault(entity);
                await Repository.InsertOneAsync(entity);
                return true;
            });
        }
        [HttpPut]
        public async virtual Task<GeneralResponse<object>> UpdateOneAsync(T entity)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await Repository.UpdateOneAsync(entity);
                return true;
            });
        }
        [HttpPut("many")]
        public async virtual Task<GeneralResponse<object>> UpdateManyAsync(ICollection<T> entities)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await Repository.UpdateManyAsync(entities);
                return true;
            });
        }
        [HttpDelete("{id}")]
        public async virtual Task<GeneralResponse<object>> DeleteOneAsync([FromRoute] U id)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await Repository.DeleteOneAsync(id);
                return true;
            });
        }
        [HttpDelete]
        public async virtual Task<GeneralResponse<object>> DeleteManyAsync([FromQuery] U[] ids)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                await Repository.DeleteManyAsync(ids);
                return true;
            });
        }

        private static void SetIdToDefault(T entity)
        {
            _ = entity.Id switch
            {
                Guid => entity.Id = (U)Convert.ChangeType(Guid.Empty, typeof(U)),
                int => entity.Id = (U)Convert.ChangeType(0, typeof(U)),
                _ => throw new Exception("Unsupported Type of Id!")
            };
        }

    }
}