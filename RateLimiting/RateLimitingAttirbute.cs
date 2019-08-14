using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace RateLimiting
{
   
    /// <summary>
    /// rate limitting attribute
    /// </summary>
    public class RateLimitingAttribute : TypeFilterAttribute
    {
        public RateLimitingAttribute() : base(typeof(RateLimitingImpl))
        {
        }
    }

    /// <summary>
    /// Per tenant rate limting implementation
    /// </summary>
    public class RateLimitingImpl : IAsyncActionFilter
    {
        private readonly IDistributedCache cache;
        private const int max= 10000;

        /// <summary>
        /// creates an instance of <see cref="RateLimitingImpl"/>
        /// </summary>
        /// <param name="_cache">IDistributed Cache</param>
        public RateLimitingImpl(IDistributedCache _cache)
        {
            this.cache = _cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Error code to be returned to the user
        /// </summary>
        public string ErrorCode { get; set; } = "maximum_service_tier_requests_reached";

        /// <summary>
        /// function that will get called when 
        /// </summary>
        /// <param name="context">the action executing context</param>
        /// <param name="next">the action execution delegate</param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            const string cacheKey = "cache_request_count_key";
            
            //obtain serialized value(IDistributed cache stores in byte arrays)
            var serialized = await cache.GetStringAsync(cacheKey);

            //if serialized is wrong then set a new cache
            if (string.IsNullOrWhiteSpace(serialized))
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(30)
                };
                await cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(new RequestCount()), cacheOptions);
            }

            //if data is null set new counter
            var current = JsonConvert.DeserializeObject<RequestCount>(serialized ?? string.Empty) ?? new RequestCount();

            //pre-check whether the request count has exceeded the request limit
            if (current.Requests >= max)
            {
                //the requests have exceeded their allowed limit 
                context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                return;
            }

            //execute request to ensure ok result
            var resultContext = await next();

            //ensure ok object
            var model = new object();
            var okResult = resultContext.Result as OkObjectResult;
            if (okResult != null) model = okResult.Value;

            // only increment on 200
            current.Requests += 1;
            serialized = JsonConvert.SerializeObject(current);

            //update the value in the cache 
            await cache.SetStringAsync(cacheKey, serialized);

            //pass the content back to user
            context.Result = new OkObjectResult(model);
        }
    }   
}
