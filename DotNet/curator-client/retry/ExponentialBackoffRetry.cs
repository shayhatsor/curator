using System;

// <summary>
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </summary>

namespace org.apache.curator.retry
{
    using VisibleForTesting = com.google.common.annotations.VisibleForTesting;
    using Logger = org.slf4j.Logger;
    using LoggerFactory = org.slf4j.LoggerFactory;

    /// <summary>
    ///     Retry policy that retries a set number of times with increasing sleep time between retries
    /// </summary>
    public class ExponentialBackoffRetry : SleepingRetry
    {
        private const int MAX_RETRIES_LIMIT = 29;
        private static readonly Logger log = LoggerFactory.getLogger(typeof (ExponentialBackoffRetry));
        private static readonly int DEFAULT_MAX_SLEEP_MS = int.MaxValue;
        private readonly int baseSleepTimeMs;
        private readonly int maxSleepMs;

        private readonly Random random = new Random();

        /// <param name="baseSleepTimeMs"> initial amount of time to wait between retries </param>
        /// <param name="maxRetries"> max number of times to retry </param>
        public ExponentialBackoffRetry(int baseSleepTimeMs, int maxRetries)
            : this(baseSleepTimeMs, maxRetries, DEFAULT_MAX_SLEEP_MS)
        {
        }

        /// <param name="baseSleepTimeMs"> initial amount of time to wait between retries </param>
        /// <param name="maxRetries"> max number of times to retry </param>
        /// <param name="maxSleepMs"> max time in ms to sleep on each retry </param>
        public ExponentialBackoffRetry(int baseSleepTimeMs, int maxRetries, int maxSleepMs)
            : base(validateMaxRetries(maxRetries))
        {
            this.baseSleepTimeMs = baseSleepTimeMs;
            this.maxSleepMs = maxSleepMs;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public int getBaseSleepTimeMs()
        public virtual int getBaseSleepTimeMs()
        {
            return baseSleepTimeMs;
        }

        protected internal override int getSleepTimeMs(int retryCount, long elapsedTimeMs)
        {
            // copied from Hadoop's RetryPolicies.java
            var sleepMs = baseSleepTimeMs*Math.Max(1, random.Next(1 << (retryCount + 1)));
            if (sleepMs > maxSleepMs)
            {
                log.warn(string.Format("Sleep extension too large ({0:D}). Pinning to {1:D}", sleepMs, maxSleepMs));
                sleepMs = maxSleepMs;
            }
            return sleepMs;
        }

        private static int validateMaxRetries(int maxRetries)
        {
            if (maxRetries > MAX_RETRIES_LIMIT)
            {
                log.warn(string.Format("maxRetries too large ({0:D}). Pinning to {1:D}", maxRetries, MAX_RETRIES_LIMIT));
                maxRetries = MAX_RETRIES_LIMIT;
            }
            return maxRetries;
        }
    }
}