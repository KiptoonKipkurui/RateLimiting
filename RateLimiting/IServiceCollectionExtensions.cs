using RateLimiting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
   public static  class IServiceCollectionExtensions
    {

        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            //add rate limiting attribute to iservice colletion
            services.AddScoped<RateLimitingAttribute>();

            //add cache...use memory cache for local debugging but use distributed cache for production environment
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            return services;
        }
    }
}
