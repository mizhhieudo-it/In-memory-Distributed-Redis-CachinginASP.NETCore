using DemoCacheMemoryAndRedis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;

namespace DemoCacheMemoryAndRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDBContext _dBContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;

        public MovieController(MovieDBContext dBContext, IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _dBContext = dBContext;
            _memoryCache = memoryCache; // injection cache in memory
            _distributedCache = distributedCache; // injection cache in redis
        }
        [HttpGet("FindMovie")]
        public async Task<IEnumerable<Movie>> FindMovie(string movieName)
        {
            #region Cache in memory
            //var cacheKey = movieName.ToLower();
            //if (!_memoryCache.TryGetValue(cacheKey, out List<Movie> movieList))
            //{
            //    movieList = await _dBContext.Movies.Where(x => x.MovieName.Contains(movieName)).ToListAsync();
            //    var cacheExpirationOptions =
            //        new MemoryCacheEntryOptions
            //        {
            //            AbsoluteExpiration = DateTime.Now.AddHours(6), // thời gian hết hạn
            //            Priority = CacheItemPriority.Normal,
            //            SlidingExpiration = TimeSpan.FromMinutes(5) // hết hạn trc 
            //        };
            //    _memoryCache.Set(cacheKey, movieList, cacheExpirationOptions);
            //}
            //return movieList;
            #endregion

            #region Cache with Redis
            var cacheKey = movieName.ToLower();
            List<Movie> listMovie;
            string serializeMovie;
            var getMovieFromRedis = await _distributedCache.GetAsync(cacheKey); // get xem trong cache redis có key cache này chưa ?
            if(getMovieFromRedis != null)
            {
                serializeMovie = Encoding.UTF8.GetString(getMovieFromRedis); // có rồi thì covert kiểu byte to string 
                listMovie = JsonConvert.DeserializeObject<List<Movie>>(serializeMovie); // convert string to json
            }
            else
            {
                // nếu chưa
                listMovie = await _dBContext.Movies.Where(x => x.MovieName.Contains(movieName)).ToListAsync(); // lấy trong db 
                // covert thành dạng json
                serializeMovie = JsonConvert.SerializeObject(listMovie);
                // chuyển từ dạng json về dạng byte 
                getMovieFromRedis =  Encoding.UTF8.GetBytes(serializeMovie);
                // setting các option 
                var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromMilliseconds(5))
                                   .SetAbsoluteExpiration(DateTime.Now.AddHours(6));
                // chuyển vào cache với key - value - và option
                await _distributedCache.SetAsync(cacheKey,getMovieFromRedis,options);

            }
            return listMovie;
                #endregion

        }
    }
}
