using System.Threading;
using System.Threading.Tasks;

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
    internal abstract class SleepingRetry : RetryPolicy
    {
        private readonly int n;

        protected internal SleepingRetry(int n)
        {
            this.n = n;
        }

        public virtual async Task<bool> allowRetry(int retryCount, long elapsedTimeMs, RetrySleeper sleeper)
        {
            if (retryCount < n)
            {
                await sleeper.sleepFor(getSleepTimeMs(retryCount, elapsedTimeMs), TimeUnit.MILLISECONDS);
                return true;
            }
            return false;
        }

        // made public for testing
        public virtual int getN()
        {
            return n;
        }

        protected internal abstract int getSleepTimeMs(int retryCount, long elapsedTimeMs);
    }
}