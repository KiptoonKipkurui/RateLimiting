using System;
using System.Collections.Generic;
using System.Text;

namespace RateLimiting
{
    /// <summary>
    /// Counts of the requests made by a tenant
    /// </summary>
    public class RequestCount
    {
        /// <summary>
        /// The actual number of requests made by a tenant
        /// </summary>
        public int Requests { get; set; }

        /// <summary>
        /// The date time the current subscription expires
        /// </summary>
        public DateTimeOffset SubscriptionExpiryTime { get; set; }
    }
}
