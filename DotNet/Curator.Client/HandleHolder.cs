﻿using System.Threading;
using System.Threading.Tasks;
using org.apache.curator.ensemble;
using org.apache.curator.utils;
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
    internal class HandleHolder
    {
        private readonly bool canBeReadOnly;
        private readonly EnsembleProvider ensembleProvider;
        private readonly int sessionTimeout;
        private readonly Watcher watcher;
        private readonly ZookeeperFactory zookeeperFactory;

        private volatile Helper helper;

        internal HandleHolder(ZookeeperFactory zookeeperFactory, Watcher watcher, EnsembleProvider ensembleProvider,
            int sessionTimeout, bool canBeReadOnly)
        {
            this.zookeeperFactory = zookeeperFactory;
            this.watcher = watcher;
            this.ensembleProvider = ensembleProvider;
            this.sessionTimeout = sessionTimeout;
            this.canBeReadOnly = canBeReadOnly;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception
        internal virtual ZooKeeper getZooKeeper()
        {
            return helper != null ? helper.getZooKeeper() : null;
        }

        internal virtual string getConnectionString()
        {
            return helper != null ? helper.getConnectionString() : null;
        }

        internal virtual bool hasNewConnectionString()
        {
            var helperConnectionString = helper != null ? helper.getConnectionString() : null;
            return (helperConnectionString != null) &&
                   !ensembleProvider.getConnectionString().Equals(helperConnectionString);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void closeAndClear() throws Exception
        internal virtual void closeAndClear()
        {
            internalClose();
            helper = null;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void closeAndReset() throws Exception
        internal virtual void closeAndReset()
        {
            internalClose();

            // first helper is synchronized when getZooKeeper is called. Subsequent calls
            // are not synchronized.
            helper = new HelperAnonymousInnerClassHelper(this);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void internalClose() throws Exception
        private Task internalClose()
        {
                var zooKeeper = helper != null ? helper.getZooKeeper() : null;
                if (zooKeeper != null)
                {
                    Watcher dummyWatcher = new WatcherAnonymousInnerClassHelper(this);
                    zooKeeper.register(dummyWatcher);
                        // clear the default watcher so that no new events get processed by mistake
                    return zooKeeper.closeAsync();
                }
            return Task.FromResult(0);
        }

        private interface Helper
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception;
            ZooKeeper getZooKeeper();

            string getConnectionString();
        }

        private class HelperAnonymousInnerClassHelper : Helper
        {
            private readonly HandleHolder outerInstance;
            private volatile string connectionString;

            private volatile ZooKeeper zooKeeperHandle;

            public HelperAnonymousInnerClassHelper(HandleHolder outerInstance)
            {
                this.outerInstance = outerInstance;
                zooKeeperHandle = null;
                connectionString = null;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception
            public virtual ZooKeeper getZooKeeper()
            {
                lock (this)
                {
                    if (zooKeeperHandle == null)
                    {
                        connectionString = outerInstance.ensembleProvider.getConnectionString();
                        zooKeeperHandle = outerInstance.zookeeperFactory.newZooKeeper(connectionString,
                            outerInstance.sessionTimeout, outerInstance.watcher, outerInstance.canBeReadOnly);
                    }

                    outerInstance.helper = new HelperAnonymousInnerClassHelper2(this);

                    return zooKeeperHandle;
                }
            }

            public virtual string getConnectionString()
            {
                return connectionString;
            }

            private class HelperAnonymousInnerClassHelper2 : Helper
            {
                private readonly HelperAnonymousInnerClassHelper outerInstance;

                public HelperAnonymousInnerClassHelper2(HelperAnonymousInnerClassHelper outerInstance)
                {
                    this.outerInstance = outerInstance;
                }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.apache.zookeeper.ZooKeeper getZooKeeper() throws Exception
                public virtual ZooKeeper getZooKeeper()
                {
                    return zooKeeperHandle;
                }

                public virtual string getConnectionString()
                {
                    return connectionString;
                }
            }
        }

        private class WatcherAnonymousInnerClassHelper : Watcher
        {
            private readonly HandleHolder outerInstance;

            public WatcherAnonymousInnerClassHelper(HandleHolder outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override Task process(WatchedEvent @event)
            {
            }
        }
    }
}