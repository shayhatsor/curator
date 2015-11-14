/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace org.apache.curator.utils
{

	using Watcher = org.apache.zookeeper.Watcher;
	using ZooKeeper = org.apache.zookeeper.ZooKeeper;

	public interface ZookeeperFactory
	{
		/// <summary>
		/// Allocate a new ZooKeeper instance
		/// 
		/// </summary>
		/// <param name="connectString"> the connection string </param>
		/// <param name="sessionTimeout"> session timeout in milliseconds </param>
		/// <param name="watcher"> optional watcher </param>
		/// <param name="canBeReadOnly"> if true, allow ZooKeeper client to enter
		///                      read only mode in case of a network partition. See
		///                      <seealso cref="ZooKeeper#ZooKeeper(String, int, Watcher, long, byte[], boolean)"/>
		///                      for details </param>
		/// <returns> the instance </returns>
		/// <exception cref="Exception"> errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.apache.zookeeper.ZooKeeper newZooKeeper(String connectString, int sessionTimeout, org.apache.zookeeper.Watcher watcher, boolean canBeReadOnly) throws Exception;
		ZooKeeper newZooKeeper(string connectString, int sessionTimeout, Watcher watcher, bool canBeReadOnly);
	}

}