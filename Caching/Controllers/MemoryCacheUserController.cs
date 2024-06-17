using Caching.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Caching.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryCacheUserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        public MemoryCacheUserController(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _cache.TryGetValue(id, out User? item);

            if(item != null)
                Log.Information("User was taken from cache");

            if(item is null)
            {
                item = await _db.Users.FindAsync(id);
                if(item != null)
                {
                    _cache.Set(item.Id, item, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                    Log.Information("User has been cached");
                }
            }

            return Ok(item);
        }

        [HttpPut]
        public async Task<IActionResult> Update(User item)
        {
            _db.Update(item);
            await _db.SaveChangesAsync();

            _cache.Remove(item.Id.ToString());

            return NoContent();
        }
    }
}