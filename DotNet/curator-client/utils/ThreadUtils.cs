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

namespace org.apache.curator.utils
{
    using ThreadFactoryBuilder = com.google.common.util.concurrent.ThreadFactoryBuilder;

    public class ThreadUtils
    {
        public static ExecutorService newSingleThreadExecutor(string processName)
        {
            return Executors.newSingleThreadExecutor(newThreadFactory(processName));
        }

        public static ExecutorService newFixedThreadPool(int qty, string processName)
        {
            return Executors.newFixedThreadPool(qty, newThreadFactory(processName));
        }

        public static ScheduledExecutorService newSingleThreadScheduledExecutor(string processName)
        {
            return Executors.newSingleThreadScheduledExecutor(newThreadFactory(processName));
        }

        public static ScheduledExecutorService newFixedThreadScheduledPool(int qty, string processName)
        {
            return Executors.newScheduledThreadPool(qty, newThreadFactory(processName));
        }

        public static ThreadFactory newThreadFactory(string processName)
        {
            return newGenericThreadFactory("Curator-" + processName);
        }

        public static ThreadFactory newGenericThreadFactory(string processName)
        {
            return new ThreadFactoryBuilder().setNameFormatuniquetempvar.setDaemon(true).build();
        }

        public static string getProcessName(Type clazz)
        {
            if (clazz.isAnonymousClass())
            {
                return getProcessName(clazz.getEnclosingClass());
            }
            return clazz.Name;
        }
    }
}