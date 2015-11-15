using System;
using System.Collections.Generic;
using System.Threading;
using org.apache.zookeeper;

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

namespace org.apache.curator
{
    using Preconditions = com.google.common.@base.Preconditions;
    using Maps = com.google.common.collect.Maps;
    using Sets = com.google.common.collect.Sets;

    /// <summary>
    ///     <para>
    ///         See <seealso cref="RetryLoop" /> for the main details on retry loops.
    ///         <b>
    ///             All Curator/ZooKeeper operations
    ///             should be done in a retry loop.
    ///         </b>
    ///     </para>
    ///     <para>
    ///         The standard retry loop treats session failure as a type of connection failure. i.e. the fact
    ///         that it is a session failure isn't considered. This can be problematic if you are performing
    ///         a series of operations that rely on ephemeral nodes. If the session fails after the ephemeral
    ///         node has been created, future Curator/ZooKeeper operations may succeed even though the
    ///         ephemeral node has been removed by ZooKeeper.
    ///     </para>
    ///     <para>
    ///         Here's an example:
    ///     </para>
    ///     <ul>
    ///         <li>You create an ephemeral/sequential node as a kind of lock/marker</li>
    ///         <li>You perform some other operations</li>
    ///         <li>The session fails for some reason</li>
    ///         <li>
    ///             You attempt to create a node assuming that the lock/marker still exists
    ///             <ul>
    ///                 <li>Curator will notice the session failure and try to reconnect</li>
    ///                 <li>
    ///                     In most cases, the reconnect will succeed and, thus, the node creation will succeed
    ///                     even though the ephemeral node will have been deleted by ZooKeeper.
    ///                 </li>
    ///             </ul>
    ///         </li>
    ///     </ul>
    ///     <para>
    ///         The SessionFailRetryLoop prevents this type of scenario. When a session failure is detected,
    ///         the thread is marked as failed which will cause all future Curator operations to fail. The
    ///         SessionFailRetryLoop will then either retry the entire
    ///         set of operations or fail (depending on <seealso cref="SessionFailRetryLoop.Mode" />)
    ///     </para>
    ///     Canonical usage:
    ///     <br>
    ///         <pre>
    ///             SessionFailRetryLoop    retryLoop = client.newSessionFailRetryLoop(mode);
    ///             retryLoop.start();
    ///             try
    ///             {
    ///             while ( retryLoop.shouldContinue() )
    ///             {
    ///             try
    ///             {
    ///             // do work
    ///             }
    ///             catch ( Exception e )
    ///             {
    ///             retryLoop.takeException(e);
    ///             }
    ///             }
    ///             }
    ///             finally
    ///             {
    ///             retryLoop.close();
    ///             }
    ///         </pre>
    /// </summary>
    public class SessionFailRetryLoop : IDisposable
    {
        public enum Mode
        {
            /// <summary>
            ///     If the session fails, retry the entire set of operations when
            ///     <seealso cref="SessionFailRetryLoop#shouldContinue()" />
            ///     is called
            /// </summary>
            RETRY,

            /// <summary>
            ///     If the session fails, throw <seealso cref="KeeperException.SessionExpiredException" /> when
            ///     <seealso cref="SessionFailRetryLoop#shouldContinue()" /> is called
            /// </summary>
            FAIL
        }

        private static readonly ISet<Thread> failedSessionThreads =
            Sets.newSetFromMap(Maps.newConcurrentMap<Thread, bool?>());

        private readonly CuratorZookeeperClient client;
        private readonly AtomicBoolean isDone = new AtomicBoolean(false);
        private readonly Mode mode;
        private readonly Thread ourThread = Thread.CurrentThread;
        private readonly RetryLoop retryLoop;
        private readonly AtomicBoolean sessionHasFailed = new AtomicBoolean(false);

        private readonly Watcher watcher = new WatcherAnonymousInnerClassHelper();

        internal SessionFailRetryLoop(CuratorZookeeperClient client, Mode mode)
        {
            this.client = client;
            this.mode = mode;
            retryLoop = client.newRetryLoop();
        }

        /// <summary>
        ///     Must be called in a finally handler when done with the loop
        /// </summary>
        public virtual void Dispose()
        {
            Preconditions.checkState(Thread.CurrentThread.Equals(ourThread), "Not in the correct thread");
            failedSessionThreads.Remove(ourThread);

            client.removeParentWatcher(watcher);
        }

        /// <summary>
        ///     Convenience utility: creates a "session fail" retry loop calling the given proc
        /// </summary>
        /// <param name="client"> Zookeeper </param>
        /// <param name="mode"> how to handle session failures </param>
        /// <param name="proc"> procedure to call with retry </param>
        /// @param
        /// <T>
        ///     return type </param>
        ///     <returns> procedure result </returns>
        ///     <exception cref="Exception"> any non-retriable errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static<T> T callWithRetry(CuratorZookeeperClient client, Mode mode, java.util.concurrent.Callable<T> proc) throws Exception
        public static T callWithRetry<T>(CuratorZookeeperClient client, Mode mode, Callable<T> proc)
        {
            T result = null;
            var retryLoop = client.newSessionFailRetryLoop(mode);
            retryLoop.start();
            try
            {
                while (retryLoop.shouldContinue())
                {
                    try
                    {
                        result = proc.call();
                    }
                    catch (Exception e)
                    {
                        retryLoop.takeException(e);
                    }
                }
            }
            finally
            {
                retryLoop.close();
            }
            return result;
        }

        internal static bool sessionForThreadHasFailed()
        {
            return (failedSessionThreads.Count > 0) && failedSessionThreads.Contains(Thread.CurrentThread);
        }

        /// <summary>
        ///     SessionFailRetryLoop must be started
        /// </summary>
        public virtual void start()
        {
            Preconditions.checkState(Thread.CurrentThread.Equals(ourThread), "Not in the correct thread");

            client.addParentWatcher(watcher);
        }

        /// <summary>
        ///     If true is returned, make an attempt at the set of operations
        /// </summary>
        /// <returns> true/false </returns>
        public virtual bool shouldContinue()
        {
            bool localIsDone = isDone.getAndSet(true);
            return !localIsDone;
        }

        /// <summary>
        ///     Pass any caught exceptions here
        /// </summary>
        /// <param name="exception"> the exception </param>
        /// <exception cref="Exception"> if not retry-able or the retry policy returned negative </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void takeException(Exception exception) throws Exception
        public virtual void takeException(Exception exception)
        {
            Preconditions.checkState(Thread.CurrentThread.Equals(ourThread), "Not in the correct thread");

            var passUp = true;
            if (sessionHasFailed.get())
            {
                switch (mode)
                {
                    case Mode.RETRY:
                    {
                        sessionHasFailed.set(false);
                        failedSessionThreads.Remove(ourThread);
                        if (exception is SessionFailedException)
                        {
                            isDone.set(false);
                            passUp = false;
                        }
                        break;
                    }

                    case Mode.FAIL:
                    {
                        break;
                    }
                }
            }

            if (passUp)
            {
                retryLoop.takeException(exception);
            }
        }

        private class WatcherAnonymousInnerClassHelper : Watcher
        {
            public override void process(WatchedEvent @event)
            {
                if (@event.getState() == Event.KeeperState.Expired)
                {
                    outerInstance.sessionHasFailed.set(true);
                    failedSessionThreads.Add(outerInstance.ourThread);
                }
            }
        }

        public class SessionFailedException : Exception
        {
            internal const long serialVersionUID = 1L;
        }
    }
}