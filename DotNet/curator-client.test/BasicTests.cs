using System.Threading;

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
namespace org.apache.curator
{

	using FixedEnsembleProvider = org.apache.curator.ensemble.@fixed.FixedEnsembleProvider;
	using RetryOneTime = org.apache.curator.retry.RetryOneTime;
	using BaseClassForTests = org.apache.curator.test.BaseClassForTests;
	using KillSession = org.apache.curator.test.KillSession;
	using Timing = org.apache.curator.test.Timing;
	using ZookeeperFactory = org.apache.curator.utils.ZookeeperFactory;
	using CreateMode = org.apache.zookeeper.CreateMode;
	using KeeperException = org.apache.zookeeper.KeeperException;
	using WatchedEvent = org.apache.zookeeper.WatchedEvent;
	using Watcher = org.apache.zookeeper.Watcher;
	using ZooDefs = org.apache.zookeeper.ZooDefs;
	using ZooKeeper = org.apache.zookeeper.ZooKeeper;
	using Mockito = org.mockito.Mockito;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class BasicTests : BaseClassForTests
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFactory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testFactory()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.zookeeper.ZooKeeper mockZookeeper = org.mockito.Mockito.mock(org.apache.zookeeper.ZooKeeper.class);
			ZooKeeper mockZookeeper = Mockito.mock(typeof(ZooKeeper));
			ZookeeperFactory zookeeperFactory = new ZookeeperFactoryAnonymousInnerClassHelper(this, mockZookeeper);
			CuratorZookeeperClient client = new CuratorZookeeperClient(zookeeperFactory, new FixedEnsembleProvider(server.getConnectString()), 10000, 10000, null, new RetryOneTime(1), false);
			client.start();
			Assert.assertEquals(client.getZooKeeper(), mockZookeeper);
		}

		private class ZookeeperFactoryAnonymousInnerClassHelper : ZookeeperFactory
		{
			private readonly BasicTests outerInstance;

			private ZooKeeper mockZookeeper;

			public ZookeeperFactoryAnonymousInnerClassHelper(BasicTests outerInstance, ZooKeeper mockZookeeper)
			{
				this.outerInstance = outerInstance;
				this.mockZookeeper = mockZookeeper;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.apache.zookeeper.ZooKeeper newZooKeeper(String connectString, int sessionTimeout, org.apache.zookeeper.Watcher watcher, boolean canBeReadOnly) throws Exception
			public virtual ZooKeeper newZooKeeper(string connectString, int sessionTimeout, Watcher watcher, bool canBeReadOnly)
			{
				return mockZookeeper;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExpiredSession() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testExpiredSession()
		{
			// see http://wiki.apache.org/hadoop/ZooKeeper/FAQ#A4

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.curator.test.Timing timing = new org.apache.curator.test.Timing();
			Timing timing = new Timing();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			CountDownLatch latch = new CountDownLatch(1);
			Watcher watcher = new WatcherAnonymousInnerClassHelper(this, latch);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), watcher, new org.apache.curator.retry.RetryOneTime(2));
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), watcher, new RetryOneTime(2));
			client.start();
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean firstTime = new java.util.concurrent.atomic.AtomicBoolean(true);
				AtomicBoolean firstTime = new AtomicBoolean(true);
				RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper(this, timing, latch, client, firstTime));
			}
			finally
			{
				client.close();
			}
		}

		private class WatcherAnonymousInnerClassHelper : Watcher
		{
			private readonly BasicTests outerInstance;

			private CountDownLatch latch;

			public WatcherAnonymousInnerClassHelper(BasicTests outerInstance, CountDownLatch latch)
			{
				this.outerInstance = outerInstance;
				this.latch = latch;
			}

			public override void process(WatchedEvent @event)
			{
				if (@event.getState() == Event.KeeperState.Expired)
				{
					latch.countDown();
				}
			}
		}

		private class CallableAnonymousInnerClassHelper : Callable<object>
		{
			private readonly BasicTests outerInstance;

			private Timing timing;
			private CountDownLatch latch;
			private org.apache.curator.CuratorZookeeperClient client;
			private AtomicBoolean firstTime;

			public CallableAnonymousInnerClassHelper(BasicTests outerInstance, Timing timing, CountDownLatch latch, org.apache.curator.CuratorZookeeperClient client, AtomicBoolean firstTime)
			{
				this.outerInstance = outerInstance;
				this.timing = timing;
				this.latch = latch;
				this.client = client;
				this.firstTime = firstTime;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object call() throws Exception
			public override object call()
			{
				if (firstTime.compareAndSet(true, false))
				{
					try
					{
						client.getZooKeeper().create("/foo", new sbyte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
					}
					catch (KeeperException.NodeExistsException)
					{
						// ignore
					}

					KillSession.kill(client.getZooKeeper(), outerInstance.server.getConnectString());

					Assert.assertTrue(timing.awaitLatch(latch));
				}
				ZooKeeper zooKeeper = client.getZooKeeper();
				client.blockUntilConnectedOrTimedOut();
				Assert.assertNotNull(zooKeeper.exists("/foo", false));
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReconnect() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testReconnect()
		{
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), 10000, 10000, null, new RetryOneTime(1));
			client.start();
			try
			{
				client.blockUntilConnectedOrTimedOut();

				sbyte[] writtenData = new sbyte[] {1, 2, 3};
				client.getZooKeeper().create("/test", writtenData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
				Thread.Sleep(1000);
				server.stop();
				Thread.Sleep(1000);

				server.restart();
				Assert.assertTrue(client.blockUntilConnectedOrTimedOut());
				sbyte[] readData = client.getZooKeeper().getData("/test", false, null);
				Assert.assertEquals(readData, writtenData);
			}
			finally
			{
				client.close();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testSimple()
		{
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), 10000, 10000, null, new RetryOneTime(1));
			client.start();
			try
			{
				client.blockUntilConnectedOrTimedOut();
				string path = client.getZooKeeper().create("/test", new sbyte[]{1,2,3}, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
				Assert.assertEquals(path, "/test");
			}
			finally
			{
				client.close();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBackgroundConnect() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBackgroundConnect()
		{
			const int CONNECTION_TIMEOUT_MS = 4000;

			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), 10000, CONNECTION_TIMEOUT_MS, null, new RetryOneTime(1));
			try
			{
				Assert.assertFalse(client.isConnected());
				client.start();

				do
				{
					for (int i = 0; i < (CONNECTION_TIMEOUT_MS / 1000); ++i)
					{
						if (client.isConnected())
						{
							goto outerBreak;
						}

						Thread.Sleep(CONNECTION_TIMEOUT_MS);
					}

					Assert.fail();
				} while (false);
					outerContinue:;
				outerBreak:;
			}
			finally
			{
				client.close();
			}
		}
	}

}