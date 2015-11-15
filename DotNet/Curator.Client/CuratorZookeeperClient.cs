using System;
using System.IO;
using org.apache.curator.drivers;
using org.apache.curator.ensemble;
using org.apache.curator.ensemble.@fixed;
using org.apache.curator.utils;
using org.apache.utils;
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
    /// <summary>
    ///     A wrapper around Zookeeper that takes care of some low-level housekeeping
    /// </summary>
    public class CuratorZookeeperClient : IDisposable
    {
        private readonly int connectionTimeoutMs;
        private static readonly TraceLogger log = TraceLogger.GetLogger(typeof (CuratorZookeeperClient));
        private readonly AtomicReference<RetryPolicy> retryPolicy = new AtomicReference<RetryPolicy>();
        private readonly AtomicBoolean started = new AtomicBoolean(false);
        private readonly ConnectionState state;

        private readonly AtomicReference<TracerDriver> tracer =
            new AtomicReference<TracerDriver>(new DefaultTracerDriver());

        /// <param name="connectString"> list of servers to connect to </param>
        /// <param name="sessionTimeoutMs"> session timeout </param>
        /// <param name="connectionTimeoutMs"> connection timeout </param>
        /// <param name="watcher"> default watcher or null </param>
        /// <param name="retryPolicy"> the retry policy to use </param>
        public CuratorZookeeperClient(string connectString, int sessionTimeoutMs, int connectionTimeoutMs,
            Watcher watcher, RetryPolicy retryPolicy)
            : this(
                new DefaultZookeeperFactory(), new FixedEnsembleProvider(connectString), sessionTimeoutMs,
                connectionTimeoutMs, watcher, retryPolicy, false)
        {
        }

        /// <param name="ensembleProvider"> the ensemble provider </param>
        /// <param name="sessionTimeoutMs"> session timeout </param>
        /// <param name="connectionTimeoutMs"> connection timeout </param>
        /// <param name="watcher"> default watcher or null </param>
        /// <param name="retryPolicy"> the retry policy to use </param>
        public CuratorZookeeperClient(EnsembleProvider ensembleProvider, int sessionTimeoutMs, int connectionTimeoutMs,
            Watcher watcher, RetryPolicy retryPolicy)
            : this(
                new DefaultZookeeperFactory(), ensembleProvider, sessionTimeoutMs, connectionTimeoutMs, watcher,
                retryPolicy, false)
        {
        }

        /// <param name="zookeeperFactory"> factory for creating <seealso cref="ZooKeeper" /> instances </param>
        /// <param name="ensembleProvider"> the ensemble provider </param>
        /// <param name="sessionTimeoutMs"> session timeout </param>
        /// <param name="connectionTimeoutMs"> connection timeout </param>
        /// <param name="watcher"> default watcher or null </param>
        /// <param name="retryPolicy"> the retry policy to use </param>
        /// <param name="canBeReadOnly">
        ///     if true, allow ZooKeeper client to enter
        ///     read only mode in case of a network partition. See
        ///     <seealso cref="ZooKeeper#ZooKeeper(String, int, Watcher, long, byte[], boolean)" />
        ///     for details
        /// </param>
        public CuratorZookeeperClient(ZookeeperFactory zookeeperFactory, EnsembleProvider ensembleProvider,
            int sessionTimeoutMs, int connectionTimeoutMs, Watcher watcher, RetryPolicy retryPolicy, bool canBeReadOnly)
        {
            if (sessionTimeoutMs < connectionTimeoutMs)
            {
                log.warn(string.Format("session timeout [{0:D}] is less than connection timeout [{1:D}]",
                    sessionTimeoutMs, connectionTimeoutMs));
            }

            retryPolicy = Preconditions.checkNotNull(retryPolicy, "retryPolicy cannot be null");
            ensembleProvider = Preconditions.checkNotNull(ensembleProvider, "ensembleProvider cannot be null");

            this.connectionTimeoutMs = connectionTimeoutMs;
            state = new ConnectionState(zookeeperFactory, ensembleProvider, sessionTimeoutMs, connectionTimeoutMs,
                watcher, tracer, canBeReadOnly);
            setRetryPolicy(retryPolicy);
        }

        /// <summary>
        ///     Close the client
        /// </summary>
        public virtual void Dispose()
        {
            log.debug("Closing");

            started.set(false);
            try
            {
                state.close();
            }
            catch (IOException e)
            {
                log.error("", e);
            }
        }

        /// <summary>
        ///     Return the managed ZK instance.
        /// </summary>
        /// <returns> client the client </returns>
        /// <exception cref="Exception"> if the connection timeout has elapsed or an exception occurs in a background process </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception
        public virtual ZooKeeper getZooKeeper()
        {
            Preconditions.checkState(started.get(), "Client is not started");

            return state.getZooKeeper();
        }

        /// <summary>
        ///     Return a new retry loop. All operations should be performed in a retry loop
        /// </summary>
        /// <returns> new retry loop </returns>
        public virtual RetryLoop newRetryLoop()
        {
            return new RetryLoop(retryPolicy.get(), tracer);
        }

        /// <summary>
        ///     Return a new "session fail" retry loop. See <seealso cref="SessionFailRetryLoop" /> for details
        ///     on when to use it.
        /// </summary>
        /// <param name="mode"> failure mode </param>
        /// <returns> new retry loop </returns>
        public virtual SessionFailRetryLoop newSessionFailRetryLoop(SessionFailRetryLoop.Mode mode)
        {
            return new SessionFailRetryLoop(this, mode);
        }

        /// <summary>
        ///     Returns true if the client is current connected
        /// </summary>
        /// <returns> true/false </returns>
        public virtual bool isConnected()
        {
            return state.isConnected();
        }

        /// <summary>
        ///     This method blocks until the connection to ZK succeeds. Use with caution. The block
        ///     will timeout after the connection timeout (as passed to the constructor) has elapsed
        /// </summary>
        /// <returns> true if the connection succeeded, false if not </returns>
        /// <exception cref="InterruptedException"> interrupted while waiting </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean blockUntilConnectedOrTimedOut() throws InterruptedException
        public virtual bool blockUntilConnectedOrTimedOut()
        {
            Preconditions.checkState(started.get(), "Client is not started");

            log.debug("blockUntilConnectedOrTimedOut() start");
            var trace = startTracer("blockUntilConnectedOrTimedOut");

            internalBlockUntilConnectedOrTimedOut();

            trace.commit();

            var localIsConnected = state.isConnected();
            log.debug("blockUntilConnectedOrTimedOut() end. isConnected: " + localIsConnected);

            return localIsConnected;
        }

        /// <summary>
        ///     Must be called after construction
        /// </summary>
        /// <exception cref="IOException"> errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws Exception
        public virtual void start()
        {
            log.debug("Starting");

            if (!started.compareAndSet(false, true))
            {
                var ise = new InvalidOperationException("Already started");
                throw ise;
            }

            state.start();
        }

        /// <summary>
        ///     Change the retry policy
        /// </summary>
        /// <param name="policy"> new policy </param>
        public virtual void setRetryPolicy(RetryPolicy policy)
        {
            Preconditions.checkNotNull(policy, "policy cannot be null");

            retryPolicy.set(policy);
        }

        /// <summary>
        ///     Return the current retry policy
        /// </summary>
        /// <returns> policy </returns>
        public virtual RetryPolicy getRetryPolicy()
        {
            return retryPolicy.get();
        }

        /// <summary>
        ///     Start a new tracer
        /// </summary>
        /// <param name="name"> name of the event </param>
        /// <returns> the new tracer (<seealso cref="TimeTrace#commit()" /> must be called) </returns>
        public virtual TimeTrace startTracer(string name)
        {
            return new TimeTrace(name, tracer.get());
        }

        /// <summary>
        ///     Return the current tracing driver
        /// </summary>
        /// <returns> tracing driver </returns>
        public virtual TracerDriver getTracerDriver()
        {
            return tracer.get();
        }

        /// <summary>
        ///     Change the tracing driver
        /// </summary>
        /// <param name="tracer"> new tracing driver </param>
        public virtual void setTracerDriver(TracerDriver tracer)
        {
            this.tracer.set(tracer);
        }

        /// <summary>
        ///     Returns the current known connection string - not guaranteed to be correct
        ///     value at any point in the future.
        /// </summary>
        /// <returns> connection string </returns>
        public virtual string getCurrentConnectionString()
        {
            return state.getEnsembleProvider().getConnectionString();
        }

        /// <summary>
        ///     Return the configured connection timeout
        /// </summary>
        /// <returns> timeout </returns>
        public virtual int getConnectionTimeoutMs()
        {
            return connectionTimeoutMs;
        }

        /// <summary>
        ///     Every time a new <seealso cref="ZooKeeper" /> instance is allocated, the "instance index"
        ///     is incremented.
        /// </summary>
        /// <returns> the current instance index </returns>
        public virtual long getInstanceIndex()
        {
            return state.getInstanceIndex();
        }

        internal virtual void addParentWatcher(Watcher watcher)
        {
            state.addParentWatcher(watcher);
        }

        internal virtual void removeParentWatcher(Watcher watcher)
        {
            state.removeParentWatcher(watcher);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void internalBlockUntilConnectedOrTimedOut() throws InterruptedException
        internal virtual void internalBlockUntilConnectedOrTimedOut()
        {
            long waitTimeMs = connectionTimeoutMs;
            while (!state.isConnected() && (waitTimeMs > 0))
            {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
                CountDownLatch latch = new CountDownLatch(1);
                Watcher tempWatcher = new WatcherAnonymousInnerClassHelper(this, latch);

                state.addParentWatcher(tempWatcher);
                var startTimeMs = TimeHelper.ElapsedMiliseconds;
                try
                {
                    latch.@await(1, TimeUnit.SECONDS);
                }
                finally
                {
                    state.removeParentWatcher(tempWatcher);
                }
                var elapsed = Math.Max(1, TimeHelper.ElapsedMiliseconds - startTimeMs);
                waitTimeMs -= elapsed;
            }
        }

        private class WatcherAnonymousInnerClassHelper : Watcher
        {
            private readonly CuratorZookeeperClient outerInstance;

            private readonly CountDownLatch latch;

            public WatcherAnonymousInnerClassHelper(CuratorZookeeperClient outerInstance, CountDownLatch latch)
            {
                this.outerInstance = outerInstance;
                this.latch = latch;
            }

            public override void process(WatchedEvent @event)
            {
                latch.countDown();
            }
        }
    }
}