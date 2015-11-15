﻿using System.Threading;
using System.Threading.Tasks;
using org.apache.utils;

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
    /// <summary>
    ///     <seealso cref="RetryPolicy" /> implementation that always <i>allowsRetry</i>.
    /// </summary>
    public class RetryForever : RetryPolicy
    {
        private readonly int retryIntervalMs;

        public RetryForever(int retryIntervalMs)
        {
            Preconditions.checkArgument(retryIntervalMs > 0);
            this.retryIntervalMs = retryIntervalMs;
        }

        public virtual async Task<bool> allowRetry(int retryCount, long elapsedTimeMs, RetrySleeper sleeper)
        {
                await sleeper.sleepFor(retryIntervalMs, TimeUnit.MILLISECONDS);
                return true;
        }
    }
}