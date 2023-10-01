using Framework.Domain.Interfaces.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Application
{
    [ApiController]
    public class CRUDController<T,U> : BaseController where T : IBaseEntity<U> where U : struct
    {
        protected IGenericRepository<T,U> Repository { get; set; }
        public CRUDController(IGenericRepository<T, U> repository)
        {
            Repository = repository;
        }

        [HttpGet]
        public async virtual Task<ActionResult<GeneralResponse<ICollection<T>>>> GetAllAsync()
        {
            try
            {
                var res = await Repository.GetAllAsync();
                return Ok(new GeneralResponse<ICollection<T>> { Data = res, LogId = Guid.Empty });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async virtual Task<ActionResult<GeneralResponse<T>>> GetByIdAsync([FromRoute] U id)
        {
            try
            {
                var res = await Repository.GetByIdAsync(id);
                return Ok(new GeneralResponse<T> { Data = res, LogId = Guid.Empty });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async virtual Task<IActionResult> InsertOneAsync(T entity)
        {
            try
            {
                SetIdToDefault(entity);
                await Repository.InsertOneAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public async virtual Task<IActionResult> UpdateOneAsync(T entity)
        {
            try
            {
                await Repository.UpdateOneAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("many")]
        public async virtual Task<IActionResult> UpdateManyAsync(ICollection<T> entities)
        {
            try
            {
                await Repository.UpdateManyAsync(entities);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async virtual Task<IActionResult> DeleteOneAsync([FromRoute] U id)
        {
            try
            {
                await Repository.DeleteOneAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async virtual Task<IActionResult> DeleteManyAsync([FromQuery] U[] ids)
        {
            try
            {
                await Repository.DeleteManyAsync(ids);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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