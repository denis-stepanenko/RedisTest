using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using Caching.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;

namespace Caching.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisCacheUserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;
        public RedisCacheUserController(ApplicationDbContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var sw = new Stopwatch();
            sw.Start();

            User? item = null;

            var json = await _cache.GetStringAsync(id.ToString());
            if (json != null)
                item = JsonSerializer.Deserialize<User>(json);

            if(item != null)
                Log.Information("User was taken from cache");

            if (item is null)
            {
                var sw2 = new Stopwatch();
                sw2.Start();
                item = await _db.Users.FindAsync(id);
                sw2.Stop();
                Log.Information($"Read from database in {sw2.ElapsedMilliseconds} ms");

                if (item != null)
                {
                    json = JsonSerializer.Serialize(item);
                    // Save object to DB for 2 minutes
                    await _cache.SetStringAsync(item.Id.ToString(), json, new DistributedCacheEntryOptions
                    {
                        // Expiration date relative to the current moment
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)

                        // AbsoluteExpiration - absolute expiration date
                        // SlidingExpiration - the time for which the record will be inactive before it is deleted
                    });

                    Log.Information("User has been cached");
                }
            }

            sw.Stop();
            Log.Information($"Returned value in {sw.ElapsedMilliseconds} ms");

            return Ok(item);
        }
    
        [HttpPut]
        public async Task<IActionResult> Update(User item)
        {
            _db.Update(item);
            await _db.SaveChangesAsync();

            await _cache.RemoveAsync(item.Id.ToString());

            return NoContent();
        }
    }
}