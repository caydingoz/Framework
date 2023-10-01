using Framework.Domain.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Entities;
using Framework.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Framework.Test.API.Controllers
{
    [ApiController]
    [Route("mongo")]
    public class MongoDbController : ControllerBase
    {
        IGenericRepositoryWithNonRelation<Log, ObjectId> MongoDbRepo { get; set; }
        public MongoDbController(IGenericRepositoryWithNonRelation<Log, ObjectId> mongoRepo)
        {
            MongoDbRepo = mongoRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var res = await MongoDbRepo.GetAllAsync();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("id")]
        public async Task<IActionResult> GetByIdAsync(ObjectId id)
        {
            try
            {
                var res = await MongoDbRepo.GetByIdAsync(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("first")]
        public async Task<IActionResult> FirstOrDefaultAsync(string msg)
        {
            try
            {
                var res = await MongoDbRepo.FirstOrDefaultAsync(x => x.Message == msg);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("single")]
        public async Task<IActionResult> SingleOrDefaultAsync(string msg)
        {
            try
            {
                var res = await MongoDbRepo.SingleOrDefaultAsync(x => x.Message == msg);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("where")]
        public async Task<IActionResult> WhereAsync(string msg, int page = 0, int count = 5)
        {
            try
            {
                var res = await MongoDbRepo.WhereAsync(x => x.Message == msg, false,
                    new Pagination { Count = count, Page = page },
                    new List<Sort> { new Sort { Name = "_id", Type = SortTypes.DESC } });
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("any")]
        public async Task<IActionResult> AnyAsync(string msg)
        {
            try
            {
                var res = await MongoDbRepo.AnyAsync(x => x.Message == msg);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("count")]
        public async Task<IActionResult> CountAsync(string msg)
        {
            try
            {
                var res = await MongoDbRepo.CountAsync(x => x.Message == msg);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> InsertOneAsync(Log entity)
        {
            try
            {
                await MongoDbRepo.InsertOneAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("many")]
        public async Task<IActionResult> InsertManyAsync(ICollection<Log> entities)
        {
            try
            {
                await MongoDbRepo.InsertManyAsync(entities);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateOneAsync(Log entity)
        {
            try
            {
                await MongoDbRepo.UpdateOneAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("many")]
        public async Task<IActionResult> UpdateManyAsync(Log[] entities)
        {
            try
            {
                await MongoDbRepo.UpdateManyAsync(entities);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteOneAsync(ObjectId id)
        {
            try
            {
                await MongoDbRepo.DeleteOneAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("many")]
        public async Task<IActionResult> UpdateManyAsync(ObjectId[] ids)
        {
            try
            {
                await MongoDbRepo.DeleteManyAsync(ids);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}