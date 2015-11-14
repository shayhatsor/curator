using System;

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

	using Lists = com.google.common.collect.Lists;
	using BaseClassForTests = org.apache.curator.test.BaseClassForTests;
	using CloseableUtils = org.apache.curator.utils.CloseableUtils;
	using ExponentialBackoffRetry = org.apache.curator.retry.ExponentialBackoffRetry;
	using RetryOneTime = org.apache.curator.retry.RetryOneTime;
	using TestingServer = org.apache.curator.test.TestingServer;
	using Timing = org.apache.curator.test.Timing;
	using CreateMode = org.apache.zookeeper.CreateMode;
	using ZooDefs = org.apache.zookeeper.ZooDefs;
	using Stat = org.apache.zookeeper.data.Stat;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class TestExhibitorEnsembleProvider : BaseClassForTests
	{
		private static readonly Exhibitors.BackupConnectionStringProvider dummyConnectionStringProvider = new BackupConnectionStringProviderAnonymousInnerClassHelper();

		private class BackupConnectionStringProviderAnonymousInnerClassHelper : Exhibitors.BackupConnectionStringProvider
		{
			public BackupConnectionStringProviderAnonymousInnerClassHelper()
			{
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getBackupConnectionString() throws Exception
			public virtual string getBackupConnectionString()
			{
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExhibitorFailures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testExhibitorFailures()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> backupConnectionString = new java.util.concurrent.atomic.AtomicReference<String>("backup1:1");
			AtomicReference<string> backupConnectionString = new AtomicReference<string>("backup1:1");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> connectionString = new java.util.concurrent.atomic.AtomicReference<String>("count=1&port=2&server0=localhost");
			AtomicReference<string> connectionString = new AtomicReference<string>("count=1&port=2&server0=localhost");
			Exhibitors exhibitors = new Exhibitors(Lists.newArrayList("foo", "bar"), 1000, new BackupConnectionStringProviderAnonymousInnerClassHelper2(this, backupConnectionString));
			ExhibitorRestClient mockRestClient = new ExhibitorRestClientAnonymousInnerClassHelper(this, connectionString);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore semaphore = new java.util.concurrent.Semaphore(0);
			Semaphore semaphore = new Semaphore(0);
			ExhibitorEnsembleProvider provider = new ExhibitorEnsembleProviderAnonymousInnerClassHelper(this, exhibitors, mockRestClient, new RetryOneTime(1), semaphore);
			provider.pollForInitialEnsemble();
			try
			{
				provider.start();

				Assert.assertEquals(provider.getConnectionString(), "localhost:2");

				connectionString.set(null);
				semaphore.drainPermits();
				semaphore.acquire(); // wait for next poll
				Assert.assertEquals(provider.getConnectionString(), "backup1:1");

				backupConnectionString.set("backup2:2");
				semaphore.drainPermits();
				semaphore.acquire(); // wait for next poll
				Assert.assertEquals(provider.getConnectionString(), "backup2:2");

				connectionString.set("count=1&port=3&server0=localhost3");
				semaphore.drainPermits();
				semaphore.acquire(); // wait for next poll
				Assert.assertEquals(provider.getConnectionString(), "localhost3:3");
			}
			finally
			{
				CloseableUtils.closeQuietly(provider);
			}
		}

		private class BackupConnectionStringProviderAnonymousInnerClassHelper2 : Exhibitors.BackupConnectionStringProvider
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private AtomicReference<string> backupConnectionString;

			public BackupConnectionStringProviderAnonymousInnerClassHelper2(TestExhibitorEnsembleProvider outerInstance, AtomicReference<string> backupConnectionString)
			{
				this.outerInstance = outerInstance;
				this.backupConnectionString = backupConnectionString;
			}

			public virtual string getBackupConnectionString()
			{
				return backupConnectionString.get();
			}
		}

		private class ExhibitorRestClientAnonymousInnerClassHelper : ExhibitorRestClient
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private AtomicReference<string> connectionString;

			public ExhibitorRestClientAnonymousInnerClassHelper(TestExhibitorEnsembleProvider outerInstance, AtomicReference<string> connectionString)
			{
				this.outerInstance = outerInstance;
				this.connectionString = connectionString;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getRaw(String hostname, int port, String uriPath, String mimeType) throws Exception
			public virtual string getRaw(string hostname, int port, string uriPath, string mimeType)
			{
				string localConnectionString = connectionString.get();
				if (localConnectionString == null)
				{
					throw new IOException();
				}
				return localConnectionString;
			}
		}

		private class ExhibitorEnsembleProviderAnonymousInnerClassHelper : ExhibitorEnsembleProvider
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private Semaphore semaphore;

			public ExhibitorEnsembleProviderAnonymousInnerClassHelper(TestExhibitorEnsembleProvider outerInstance, org.apache.curator.ensemble.exhibitor.Exhibitors exhibitors, org.apache.curator.ensemble.exhibitor.ExhibitorRestClient mockRestClient, RetryOneTime org, Semaphore semaphore) : base(exhibitors, mockRestClient, "/foo", 10, RetryOneTime)
			{
				this.outerInstance = outerInstance;
				this.semaphore = semaphore;
			}

			protected internal override void poll()
			{
				base.poll();
				semaphore.release();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChanging() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testChanging()
		{
			TestingServer secondServer = new TestingServer();
			try
			{
				string mainConnectionString = "count=1&port=" + server.getPort() + "&server0=localhost";
				string secondConnectionString = "count=1&port=" + secondServer.getPort() + "&server0=localhost";

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore semaphore = new java.util.concurrent.Semaphore(0);
				Semaphore semaphore = new Semaphore(0);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> connectionString = new java.util.concurrent.atomic.AtomicReference<String>(mainConnectionString);
				AtomicReference<string> connectionString = new AtomicReference<string>(mainConnectionString);
				Exhibitors exhibitors = new Exhibitors(Lists.newArrayList("foo", "bar"), 1000, dummyConnectionStringProvider);
				ExhibitorRestClient mockRestClient = new ExhibitorRestClientAnonymousInnerClassHelper2(this, semaphore, connectionString);
				ExhibitorEnsembleProvider provider = new ExhibitorEnsembleProvider(exhibitors, mockRestClient, "/foo", 10, new RetryOneTime(1));
				provider.pollForInitialEnsemble();

				Timing timing = (new Timing()).multiple(4);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.curator.CuratorZookeeperClient client = new org.apache.curator.CuratorZookeeperClient(provider, timing.session(), timing.connection(), null, new org.apache.curator.retry.RetryOneTime(2));
				CuratorZookeeperClient client = new CuratorZookeeperClient(provider, timing.session(), timing.connection(), null, new RetryOneTime(2));
				client.start();
				try
				{
					RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper(this, client));

					connectionString.set(secondConnectionString);
					semaphore.drainPermits();
					semaphore.acquire();

					server.stop(); // create situation where the current zookeeper gets a sys-disconnected

					Stat stat = RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper2(this, client));
					Assert.assertNull(stat); // it's a different server so should be null
				}
				finally
				{
					client.close();
				}
			}
			finally
			{
				CloseableUtils.closeQuietly(secondServer);
			}
		}

		private class ExhibitorRestClientAnonymousInnerClassHelper2 : ExhibitorRestClient
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private Semaphore semaphore;
			private AtomicReference<string> connectionString;

			public ExhibitorRestClientAnonymousInnerClassHelper2(TestExhibitorEnsembleProvider outerInstance, Semaphore semaphore, AtomicReference<string> connectionString)
			{
				this.outerInstance = outerInstance;
				this.semaphore = semaphore;
				this.connectionString = connectionString;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getRaw(String hostname, int port, String uriPath, String mimeType) throws Exception
			public virtual string getRaw(string hostname, int port, string uriPath, string mimeType)
			{
				semaphore.release();
				return connectionString.get();
			}
		}

		private class CallableAnonymousInnerClassHelper : Callable<object>
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private CuratorZookeeperClient client;

			public CallableAnonymousInnerClassHelper(TestExhibitorEnsembleProvider outerInstance, CuratorZookeeperClient client)
			{
				this.outerInstance = outerInstance;
				this.client = client;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object call() throws Exception
			public override object call()
			{
				client.getZooKeeper().create("/test", new sbyte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
				return null;
			}
		}

		private class CallableAnonymousInnerClassHelper2 : Callable<Stat>
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			private CuratorZookeeperClient client;

			public CallableAnonymousInnerClassHelper2(TestExhibitorEnsembleProvider outerInstance, CuratorZookeeperClient client)
			{
				this.outerInstance = outerInstance;
				this.client = client;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public org.apache.zookeeper.data.Stat call() throws Exception
			public override Stat call()
			{
				return client.getZooKeeper().exists("/test", false);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testSimple()
		{
			Exhibitors exhibitors = new Exhibitors(Lists.newArrayList("foo", "bar"), 1000, dummyConnectionStringProvider);
			ExhibitorRestClient mockRestClient = new ExhibitorRestClientAnonymousInnerClassHelper3(this);
			ExhibitorEnsembleProvider provider = new ExhibitorEnsembleProvider(exhibitors, mockRestClient, "/foo", 10, new RetryOneTime(1));
			provider.pollForInitialEnsemble();

			Timing timing = new Timing();
			CuratorZookeeperClient client = new CuratorZookeeperClient(provider, timing.session(), timing.connection(), null, new ExponentialBackoffRetry(timing.milliseconds(), 3));
			client.start();
			try
			{
				client.blockUntilConnectedOrTimedOut();
				client.getZooKeeper().exists("/", false);
			}
			catch (Exception e)
			{
				Assert.fail("provider.getConnectionString(): " + provider.getConnectionString() + " server.getPort(): " + server.getPort(), e);
			}
			finally
			{
				client.close();
			}
		}

		private class ExhibitorRestClientAnonymousInnerClassHelper3 : ExhibitorRestClient
		{
			private readonly TestExhibitorEnsembleProvider outerInstance;

			public ExhibitorRestClientAnonymousInnerClassHelper3(TestExhibitorEnsembleProvider outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getRaw(String hostname, int port, String uriPath, String mimeType) throws Exception
			public virtual string getRaw(string hostname, int port, string uriPath, string mimeType)
			{
				return "count=1&port=" + outerInstance.server.getPort() + "&server0=localhost";
			}
		}
	}

}