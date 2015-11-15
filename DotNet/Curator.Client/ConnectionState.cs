using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using org.apache.curator.drivers;
using org.apache.curator.ensemble;
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
    internal class ConnectionState : Watcher
    {
        private const int MAX_BACKGROUND_EXCEPTIONS = 10;
        private static readonly bool LOG_EVENTS = bool.getBoolean(DebugUtils.PROPERTY_LOG_EVENTS);
        private static readonly TraceLogger log = TraceLogger.GetLogger(typeof(ConnectionState));
        private readonly LinkedList<Exception> backgroundExceptions = new ConcurrentLinkedQueue<Exception>();
        private readonly int connectionTimeoutMs;
        private readonly EnsembleProvider ensembleProvider;
        private readonly AtomicLong instanceIndex = new AtomicLong();
        private readonly AtomicBoolean isConnected_Renamed = new AtomicBoolean(false);
        private readonly LinkedList<Watcher> parentWatchers = new ConcurrentLinkedQueue<Watcher>();
        private readonly int sessionTimeoutMs;
        private readonly AtomicReference<TracerDriver> tracer;
        private readonly HandleHolder zooKeeper;
        private AtomicLong connectionStartMs = new AtomicLong();

        internal ConnectionState(ZookeeperFactory zookeeperFactory, EnsembleProvider ensembleProvider,
            int sessionTimeoutMs, int connectionTimeoutMs, Watcher parentWatcher, AtomicReference<TracerDriver> tracer,
            bool canBeReadOnly)
        {
            this.ensembleProvider = ensembleProvider;
            this.sessionTimeoutMs = sessionTimeoutMs;
            this.connectionTimeoutMs = connectionTimeoutMs;
            this.tracer = tracer;
            if (parentWatcher != null)
            {
                parentWatchers.AddLast(parentWatcher);
            }

            zooKeeper = new HandleHolder(zookeeperFactory, this, ensembleProvider, sessionTimeoutMs, canBeReadOnly);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
        public virtual void close()
        {
            log.debug("Closing");

            CloseableUtils.closeQuietly(ensembleProvider);
            try
            {
                zooKeeper.closeAndClear();
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
            finally
            {
                isConnected_Renamed.set(false);
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception
        internal virtual ZooKeeper getZooKeeper()
        {
            if (SessionFailRetryLoop.sessionForThreadHasFailed())
            {
                throw new SessionFailRetryLoop.SessionFailedException();
            }

            Exception exception = backgroundExceptions.RemoveFirst();
            if (exception != null)
            {
                tracer.get().addCount("background-exceptions", 1);
                throw exception;
            }

            bool localIsConnected = isConnected_Renamed.get();
            if (!localIsConnected)
            {
                checkTimeouts();
            }

            return zooKeeper.getZooKeeper();
        }

        internal virtual bool isConnected()
        {
            return isConnected_Renamed.get();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void start() throws Exception
        internal virtual void start()
        {
            log.debug("Starting");
            ensembleProvider.start();
            reset();
        }

        internal virtual void addParentWatcher(Watcher watcher)
        {
            parentWatchers.AddLast(watcher);
        }

        internal virtual void removeParentWatcher(Watcher watcher)
        {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
            parentWatchers.remove(watcher);
        }

        internal virtual long getInstanceIndex()
        {
            return instanceIndex.get();
        }

        public override Task process(WatchedEvent @event)
        {
            if (LOG_EVENTS)
            {
                log.debug("ConnectState watcher: " + @event);
            }

            if (@event.get_Type() == Event.EventType.None)
            {
                bool wasConnected = isConnected_Renamed.get();
                var newIsConnected = checkState(@event.getState(), wasConnected);
                if (newIsConnected != wasConnected)
                {
                    isConnected_Renamed.set(newIsConnected);
                    connectionStartMs.Value = TimeHelper.ElapsedMiliseconds;
                }
            }

            foreach (var parentWatcher in parentWatchers)
            {
                var timeTrace = new TimeTrace("connection-state-parent-process", tracer.get());
                parentWatcher.process(@event);
                timeTrace.commit();
            }
        }

        internal virtual EnsembleProvider getEnsembleProvider()
        {
            return ensembleProvider;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private synchronized void checkTimeouts() throws Exception
        private void checkTimeouts()
        {
            lock (this)
            {
                var minTimeout = Math.Min(sessionTimeoutMs, connectionTimeoutMs);
                var elapsed = TimeHelper.ElapsedMiliseconds - connectionStartMs.get();
                if (elapsed >= minTimeout)
                {
                    if (zooKeeper.hasNewConnectionString())
                    {
                        handleNewConnectionString();
                    }
                    else
                    {
                        var maxTimeout = Math.Max(sessionTimeoutMs, connectionTimeoutMs);
                        if (elapsed > maxTimeout)
                        {
                            if (!bool.getBoolean(DebugUtils.PROPERTY_DONT_LOG_CONNECTION_ISSUES))
                            {
                                log.warn(
                                    string.Format(
                                        "Connection attempt unsuccessful after {0:D} (greater than max timeout of {1:D}). Resetting connection and trying again with a new connection.",
                                        elapsed, maxTimeout));
                            }
                            reset();
                        }
                        else
                        {
                            KeeperException.ConnectionLossException connectionLossException =
                                new CuratorConnectionLossException();
                            if (!bool.getBoolean(DebugUtils.PROPERTY_DONT_LOG_CONNECTION_ISSUES))
                            {
                                log.error(
                                    string.Format(
                                        "Connection timed out for connection string ({0}) and timeout ({1:D}) / elapsed ({2:D})",
                                        zooKeeper.getConnectionString(), connectionTimeoutMs, elapsed),
                                    connectionLossException);
                            }
                            tracer.get().addCount("connections-timed-out", 1);
                            throw connectionLossException;
                        }
                    }
                }
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private synchronized void reset() throws Exception
        private void reset()
        {
            lock (this)
            {
                log.debug("reset");

                instanceIndex.incrementAndGet();

                isConnected_Renamed.set(false);
                connectionStartMs.Value = TimeHelper.ElapsedMiliseconds;
                zooKeeper.closeAndReset();
                zooKeeper.getZooKeeper(); // initiate connection
            }
        }

        private bool checkState(Event.KeeperState state, bool wasConnected)
        {
            var isConnected = wasConnected;
            var checkNewConnectionString = true;
            switch (state)
            {
                default:
                case Event.KeeperState.Disconnected:
                {
                    isConnected = false;
                    break;
                }

                case Event.KeeperState.SyncConnected:
                case Event.KeeperState.ConnectedReadOnly:
                {
                    isConnected = true;
                    break;
                }

                case Event.KeeperState.AuthFailed:
                {
                    isConnected = false;
                    log.error("Authentication failed");
                    break;
                }

                case Event.KeeperState.Expired:
                {
                    isConnected = false;
                    checkNewConnectionString = false;
                    handleExpiredSession();
                    break;
                }
            }

            if (checkNewConnectionString && zooKeeper.hasNewConnectionString())
            {
                handleNewConnectionString();
            }

            return isConnected;
        }

        private void handleNewConnectionString()
        {
            log.info("Connection string changed");
            tracer.get().addCount("connection-string-changed", 1);

            try
            {
                reset();
            }
            catch (Exception e)
            {
                queueBackgroundException(e);
            }
        }

        private void handleExpiredSession()
        {
            log.warn("Session expired event received");
            tracer.get().addCount("session-expired", 1);

            try
            {
                reset();
            }
            catch (Exception e)
            {
                queueBackgroundException(e);
            }
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"ThrowableResultOfMethodCallIgnored"}) private void queueBackgroundException(Exception e)
        private void queueBackgroundException(Exception e)
        {
            while (backgroundExceptions.Count >= MAX_BACKGROUND_EXCEPTIONS)
            {
                backgroundExceptions.RemoveFirst();
            }
            backgroundExceptions.AddLast(e);
        }
    }
}