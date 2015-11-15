using System;
using System.Collections.Generic;

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
    ///     Decoration on an ExecutorService that tracks created futures and provides
    ///     a method to close futures created via this class
    /// </summary>
    public class CloseableExecutorService : IDisposable
    {
        private readonly ExecutorService executorService;
        protected internal readonly AtomicBoolean isOpen = new AtomicBoolean(true);
        private readonly Logger log = LoggerFactory.getLogger(typeof (CloseableExecutorService));
        private readonly bool shutdownOnClose;

        /// <param name="executorService"> the service to decorate </param>
        public CloseableExecutorService(ExecutorService executorService) : this(executorService, false)
        {
        }

        /// <param name="executorService"> the service to decorate </param>
        /// <param name="shutdownOnClose"> if true, shutdown the executor service when this is closed </param>
        public CloseableExecutorService(ExecutorService executorService, bool shutdownOnClose)
        {
            this.executorService = Preconditions.checkNotNull(executorService, "executorService cannot be null");
            this.shutdownOnClose = shutdownOnClose;
        }

        private submit(Runnable task)
        {
            Preconditions.checkState(isOpen.get(), "CloseableExecutorService is closed");

            InternalFutureTask<void> futureTask = new InternalFutureTask<void>(new FutureTask<void>(task, null));
            executorService.execute(futureTask);
            return futureTask;
        }

        /// <summary>
        ///     Closes any tasks currently in progress
        /// </summary>
        public virtual void Dispose()
        {
            isOpen.set(false);
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.Iterator<Future<?>> iterator = futures.iterator();
            IEnumerator < Future < ? >>
            iterator = futures.GetEnumerator();
            while (iterator.MoveNext())
            {
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: Future<?> future = iterator.Current;
                Future < ? >
                future = iterator.Current;
                iterator.remove();
                if (!future.isDone() && !future.isCancelled() && !future.cancel(true))
                {
                    log.warn("Could not cancel " + future);
                }
            }
            if (shutdownOnClose)
            {
                executorService.shutdownNow();
            }
        }

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private final java.util.Set<Future<?>> futures = com.google.common.collect.Sets.newSetFromMap(com.google.common.collect.Maps.newConcurrentMap<Future<?>, Boolean>());
        private readonly ISet<Future<?> 
    >
        futures 
    =
        Sets.newSetFromMap 
    (
        Maps.newConcurrentMap<Future<?> 
    ,
        bool? 
    >());

        /// <summary>
        ///     Returns <tt>true</tt> if this executor has been shut down.
        /// </summary>
        /// <returns> <tt>true</tt> if this executor has been shut down </returns>
        public virtual bool isShutdown()
        {
            return !isOpen.get();
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int size()
        internal virtual int size()
        {
            return futures.Count;
        }

        /// <summary>
        ///     Submits a value-returning task for execution and returns a Future
        ///     representing the pending results of the task.  Upon completion,
        ///     this task may be taken or polled.
        /// </summary>
        /// <param name="task"> the task to submit </param>
        /// <returns> a future to watch the task </returns>
        public virtual Future<V> submit<V>(Callable<V> task)
        {
            Preconditions.checkState(isOpen.get(), "CloseableExecutorService is closed");

            var futureTask = new InternalFutureTask<V>(new FutureTask<V>(task));
            executorService.execute(futureTask);
            return futureTask;
        }

        /// <summary>
        ///     Submits a Runnable task for execution and returns a Future
        ///     representing that task.  Upon completion, this task may be
        ///     taken or polled.
        /// </summary>
        /// <param name="task"> the task to submit </param>
        /// <returns> a future to watch the task </returns>
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public Future<?> submit(Runnable task)
        public virtual Future<? 

    >

        protected internal class InternalScheduledFutureTask : Future<void>
        {
            private readonly CloseableExecutorService outerInstance;
            private CloseableExecutorService outerInstance
            private CloseableExecutorService ScheduledFuture

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private final ScheduledFuture<?> scheduledFuture;
            internal readonly ScheduledFuture<? 
        >
            scheduledFuture ;

            public InternalScheduledFutureTask<T1> 
        (
        <
            T1 
        >
            scheduledFuture 
        )

            public override bool cancel(bool mayInterruptIfRunning)
            {
                outerInstance.futures.Remove(scheduledFuture);
                return scheduledFuture.cancel(mayInterruptIfRunning);
            }

            public override bool isCancelled()
            {
                return scheduledFuture.isCancelled();
            }

            public override bool isDone()
            {
                return scheduledFuture.isDone();
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void get() throws InterruptedException, ExecutionException
            public override void get()
            {
                return null;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void get(long timeout, TimeUnit unit) throws InterruptedException, ExecutionException, TimeoutException
            public override void get(long timeout, TimeUnit unit)
            {
                return null;
            }
        }

        protected internal class InternalFutureTask<T> : FutureTask<T>
        {
            private readonly CloseableExecutorService outerInstance;

            internal readonly RunnableFuture<T> task;

            internal InternalFutureTask(CloseableExecutorService outerInstance, RunnableFuture<T> task)
                : base(task, null)
            {
                this.outerInstance = outerInstance;
                this.task = task;
                outerInstance.futures.Add(task);
            }

            protected internal virtual void done()
            {
                outerInstance.futures.Remove(task);
            }
        }
    }
}