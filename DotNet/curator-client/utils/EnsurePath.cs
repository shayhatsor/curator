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
    /// <summary>
    ///     <para>
    ///         Utility to ensure that a particular path is created.
    ///     </para>
    ///     <para>
    ///         The first time it is used, a synchronized call to <seealso cref="ZKPaths#mkdirs(ZooKeeper, String)" /> is made
    ///         to
    ///         ensure that the entire path has been created (with an empty byte array if needed). Subsequent
    ///         calls with the instance are un-synchronized NOPs.
    ///     </para>
    ///     <para>
    ///         Usage:<br>
    ///     </para>
    ///     <pre>
    ///         EnsurePath       ensurePath = new EnsurePath(aFullPathToEnsure);
    ///         ...
    ///         String           nodePath = aFullPathToEnsure + "/foo";
    ///         ensurePath.ensure(zk);   // first time syncs and creates if needed
    ///         zk.create(nodePath, ...);
    ///         ...
    ///         ensurePath.ensure(zk);   // subsequent times are NOPs
    ///         zk.create(nodePath, ...);
    ///     </pre>
    /// </summary>
    /// @deprecated Since 2.9.0 - Prefer CuratorFramework.create().creatingParentContainersIfNeeded() or CuratorFramework.exists().creatingParentContainersIfNeeded()
    [Obsolete(
        "Since 2.9.0 - Prefer CuratorFramework.create().creatingParentContainersIfNeeded() or CuratorFramework.exists().creatingParentContainersIfNeeded()"
        )]
    public class EnsurePath
    {
        private static readonly Helper doNothingHelper = new HelperAnonymousInnerClassHelper();
        private readonly InternalACLProvider aclProvider;
        private readonly AtomicReference<Helper> helper;
        private readonly bool makeLastNode;
        private readonly string path;

        /// <param name="path"> the full path to ensure </param>
        public EnsurePath(string path) : this(path, null, true, null)
        {
        }

        /// <param name="path"> the full path to ensure </param>
        /// <param name="aclProvider"> if not null, the ACL provider to use when creating parent nodes </param>
        public EnsurePath(string path, InternalACLProvider aclProvider) : this(path, null, true, aclProvider)
        {
        }

        protected internal EnsurePath(string path, AtomicReference<Helper> helper, bool makeLastNode,
            InternalACLProvider aclProvider)
        {
            this.path = path;
            this.makeLastNode = makeLastNode;
            this.aclProvider = aclProvider;
            this.helper = helper != null ? helper : new AtomicReference<Helper>(new InitialHelper(this));
        }

        /// <summary>
        ///     First time, synchronizes and makes sure all nodes in the path are created. Subsequent calls
        ///     with this instance are NOPs.
        /// </summary>
        /// <param name="client"> ZK client </param>
        /// <exception cref="Exception"> ZK errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void ensure(org.apache.curator.CuratorZookeeperClient client) throws Exception
        public virtual void ensure(CuratorZookeeperClient client)
        {
            Helper localHelper = helper.get();
            localHelper.ensure(client, path, makeLastNode);
        }

        /// <summary>
        ///     Returns a view of this EnsurePath instance that does not make the last node.
        ///     i.e. if the path is "/a/b/c" only "/a/b" will be ensured
        /// </summary>
        /// <returns> view </returns>
        public virtual EnsurePath excludingLast()
        {
            return new EnsurePath(path, helper, false, aclProvider);
        }

        /// <summary>
        ///     Returns the path being Ensured
        /// </summary>
        /// <returns> the path being ensured </returns>
        public virtual string getPath()
        {
            return path;
        }

        protected internal virtual bool asContainers()
        {
            return false;
        }

        private class HelperAnonymousInnerClassHelper : Helper
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void ensure(org.apache.curator.CuratorZookeeperClient client, String path, final boolean makeLastNode) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
            public virtual void ensure(CuratorZookeeperClient client, string path, bool makeLastNode)
            {
                // NOP
            }
        }

        internal interface Helper
        {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void ensure(org.apache.curator.CuratorZookeeperClient client, String path, final boolean makeLastNode) throws Exception;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
            void ensure(CuratorZookeeperClient client, string path, bool makeLastNode);
        }

        private class InitialHelper : Helper
        {
            private readonly EnsurePath outerInstance;

            internal bool isSet; // guarded by synchronization

            public InitialHelper(EnsurePath outerInstance)
            {
                this.outerInstance = outerInstance;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public synchronized void ensure(final org.apache.curator.CuratorZookeeperClient client, final String path, final boolean makeLastNode) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
            public virtual void ensure(CuratorZookeeperClient client, string path, bool makeLastNode)
            {
                lock (this)
                {
                    if (!isSet)
                    {
                        RetryLoop.callWithRetry(client,
                            new CallableAnonymousInnerClassHelper(this, client, path, makeLastNode));
                    }
                }
            }

            private class CallableAnonymousInnerClassHelper : Callable<object>
            {
                private readonly InitialHelper outerInstance;

                private readonly CuratorZookeeperClient client;
                private readonly bool makeLastNode;
                private readonly string path;

                public CallableAnonymousInnerClassHelper(InitialHelper outerInstance, CuratorZookeeperClient client,
                    string path, bool makeLastNode)
                {
                    this.outerInstance = outerInstance;
                    this.client = client;
                    this.path = path;
                    this.makeLastNode = makeLastNode;
                }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object call() throws Exception
                public override object call()
                {
                    ZKPaths.mkdirs(client.getZooKeeper(), path, makeLastNode, outerInstance.outerInstance.aclProvider,
                        outerInstance.outerInstance.asContainers());
                    outerInstance.outerInstance.helper.set(doNothingHelper);
                    outerInstance.isSet = true;
                    return null;
                }
            }
        }
    }
}