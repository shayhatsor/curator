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

using org.apache.curator.drivers;
using org.apache.utils;

namespace org.apache.curator.utils
{

    /// <summary>
    ///     Default tracer driver
    /// </summary>
    internal class DefaultTracerDriver : TracerDriver
    {
        private static readonly TraceLogger log = TraceLogger.GetLogger(typeof(DefaultTracerDriver));

        public virtual void addTrace(string name, long time, TimeUnit unit)
        {
            if (log.isTraceEnabled())
            {
                log.trace("Trace: " + name + " - " + TimeUnit.MILLISECONDS.convert(time, unit) + " ms");
            }
        }

        public virtual void addCount(string name, int increment)
        {
            if (log.isTraceEnabled())
            {
                log.trace("Counter " + name + ": " + increment);
            }
        }
    }
}