using System.Collections.Generic;

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
namespace org.apache.curator.ensemble.exhibitor
{

	using Preconditions = com.google.common.@base.Preconditions;
	using ImmutableList = com.google.common.collect.ImmutableList;

	/// <summary>
	/// POJO for specifying the cluster of Exhibitor instances
	/// </summary>
	public class Exhibitors
	{
		private readonly ICollection<string> hostnames;
		private readonly int restPort;
		private readonly BackupConnectionStringProvider backupConnectionStringProvider;

		public interface BackupConnectionStringProvider
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getBackupConnectionString() throws Exception;
			string getBackupConnectionString();
		}

		/// <param name="hostnames"> set of Exhibitor instance host names </param>
		/// <param name="restPort"> the REST port used to connect to Exhibitor </param>
		/// <param name="backupConnectionStringProvider"> in case an Exhibitor instance can't be contacted, returns the fixed
		///                               connection string to use as a backup </param>
		public Exhibitors(ICollection<string> hostnames, int restPort, BackupConnectionStringProvider backupConnectionStringProvider)
		{
			this.backupConnectionStringProvider = Preconditions.checkNotNull(backupConnectionStringProvider, "backupConnectionStringProvider cannot be null");
			this.hostnames = ImmutableList.copyOf(hostnames);
			this.restPort = restPort;
		}

		public virtual ICollection<string> getHostnames()
		{
			return hostnames;
		}

		public virtual int getRestPort()
		{
			return restPort;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getBackupConnectionString() throws Exception
		public virtual string getBackupConnectionString()
		{
			return backupConnectionStringProvider.getBackupConnectionString();
		}
	}

}