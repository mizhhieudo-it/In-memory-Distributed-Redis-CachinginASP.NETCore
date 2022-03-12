using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace DemoCacheMemoryAndRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _distributedCache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get(string caching)
        {
            var cacheKey = caching;
            string serializedCustomerList;
            List<string> weatherList = new List<string>();
            var redisCustomerList = await _distributedCache.GetAsync(cacheKey);
            if (redisCustomerList != null)
            {
                serializedCustomerList = Encoding.UTF8.GetString(redisCustomerList);
                weatherList = JsonConvert.DeserializeObject<List<string>>(serializedCustomerList);
                weatherList.Add("Hi Add from Redis Cache");
          ///      await _distributedCache.RemoveAsync(cacheKey); => Remove Cache with key
          ///         await _distributedCache.RefreshAsync(cacheKey); => Reset Cache with key 
            }
            else
            {
                weatherList = Summaries.ToList();
                weatherList.Add("Hi Data get from DB");
                serializedCustomerList = JsonConvert.SerializeObject(weatherList);
                redisCustomerList = Encoding.UTF8.GetBytes(serializedCustomerList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(10)).SetSlidingExpiration(TimeSpan.FromMinutes(5));
                await _distributedCache.SetAsync(cacheKey, redisCustomerList, options);

            }
            return Ok(weatherList);
        }
    }
}