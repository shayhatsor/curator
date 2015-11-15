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
    /// <summary>
    ///     Decoration on an ScheduledExecutorService that tracks created futures and provides
    ///     a method to close futures created via this class
    /// </summary>
    public class CloseableScheduledExecutorService : CloseableExecutorService
    {
        private readonly ScheduledExecutorService scheduledExecutorService;

        /// <param name="scheduledExecutorService"> the service to decorate </param>
        public CloseableScheduledExecutorService(ScheduledExecutorService scheduledExecutorService)
            : base(scheduledExecutorService, false)
        {
            this.scheduledExecutorService = scheduledExecutorService;
        }

        /// <param name="scheduledExecutorService"> the service to decorate </param>
        /// <param name="shutdownOnClose"> if true, shutdown the executor service when this is closed </param>
        public CloseableScheduledExecutorService(ScheduledExecutorService scheduledExecutorService, bool shutdownOnClose)
            : base(scheduledExecutorService, shutdownOnClose)
        {
            this.scheduledExecutorService = scheduledExecutorService;
        }

        private schedule(Runnable task, long delay, TimeUnit unit)
        {
            Preconditions.checkState(isOpen.get(), "CloseableExecutorService is closed");

            InternalFutureTask<void> futureTask = new InternalFutureTask<void>(new FutureTask<void>(task, null));
            scheduledExecutorService.schedule(futureTask, delay, unit);
            return futureTask;
        }

        private scheduleWithFixedDelay(Runnable task, long initialDelay, long delay, TimeUnit unit)
        {
            Preconditions.checkState(isOpen.get(), "CloseableExecutorService is closed");

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.concurrent.ScheduledFuture<?> scheduledFuture = scheduledExecutorService.scheduleWithFixedDelay(task, initialDelay, delay, unit);
            ScheduledFuture < ? >
            scheduledFuture = scheduledExecutorService.scheduleWithFixedDelay(task, initialDelay, delay, unit);
            return new InternalScheduledFutureTask(this, scheduledFuture);
        }

        /// <summary>
        ///     Creates and executes a one-shot action that becomes enabled
        ///     after the given delay.
        /// </summary>
        /// <param name="task">  the task to execute </param>
        /// <param name="delay"> the time from now to delay execution </param>
        /// <param name="unit">  the time unit of the delay parameter </param>
        /// <returns>
        ///     a Future representing pending completion of
        ///     the task and whose <tt>get()</tt> method will return
        ///     <tt>null</tt> upon completion
        /// </returns>
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public java.util.concurrent.Future<?> schedule(Runnable task, long delay, java.util.concurrent.TimeUnit unit)
        public virtual Future<? 

    >

        /// <summary>
        ///     Creates and executes a periodic action that becomes enabled first
        ///     after the given initial delay, and subsequently with the
        ///     given delay between the termination of one execution and the
        ///     commencement of the next.  If any execution of the task
        ///     encounters an exception, subsequent executions are suppressed.
        ///     Otherwise, the task will only terminate via cancellation or
        ///     termination of the executor.
        /// </summary>
        /// <param name="task">      the task to execute </param>
        /// <param name="initialDelay"> the time to delay first execution </param>
        /// <param name="delay">
        ///     the delay between the termination of one
        ///     execution and the commencement of the next
        /// </param>
        /// <param name="unit">         the time unit of the initialDelay and delay parameters </param>
        /// <returns>
        ///     a Future representing pending completion of
        ///     the task, and whose <tt>get()</tt> method will throw an
        ///     exception upon cancellation
        /// </returns>
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public java.util.concurrent.Future<?> scheduleWithFixedDelay(Runnable task, long initialDelay, long delay, java.util.concurrent.TimeUnit unit)
        public virtual Future<? 

    >
    }
}