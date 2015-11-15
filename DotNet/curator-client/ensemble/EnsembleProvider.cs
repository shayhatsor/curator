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


using System;
using System.IO;
using org.apache.zookeeper;

namespace org.apache.curator.ensemble
{
    /// <summary>
    ///     Abstraction that provides the ZooKeeper connection string
    /// </summary>
    public interface EnsembleProvider : IDisposable
    {
        /// <summary>
        ///     Curator will call this method when <seealso cref="CuratorZookeeperClient#start()" /> is
        ///     called
        /// </summary>
        void start();

        /// <summary>
        ///     Return the current connection string to use. Curator will call this each
        ///     time it needs to create a ZooKeeper instance
        /// </summary>
        /// <returns> connection string (per <seealso cref="ZooKeeper#ZooKeeper(String, int, Watcher)" /> etc.) </returns>
        string getConnectionString();

        /// <summary>
        ///     Curator will call this method when <seealso cref="CuratorZookeeperClient#close()" /> is called
        /// </summary>
        /// <exception cref="IOException"> errors </exception>
        void close();
    }
}