using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.CosmosDB
{
    public class CosmosRetryOptions
    {
        public int MaxConnectionLimit { get; set; }
        public TimeSpan RequestTimeout { get; set; }
        public int MaxRetryAttemptsOnThrottledRequests { get; set; }
        public int MaxRetryWaitTimeInSeconds { get; set; }
    }
}
