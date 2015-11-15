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

namespace org.apache.curator
{

	using BaseClassForTests = org.apache.curator.test.BaseClassForTests;
	using CloseableUtils = org.apache.curator.utils.CloseableUtils;
	using RetryOneTime = org.apache.curator.retry.RetryOneTime;
	using KillSession = org.apache.curator.test.KillSession;
	using Timing = org.apache.curator.test.Timing;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class TestSessionFailRetryLoop : BaseClassForTests
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRetry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRetry()
		{
			Timing timing = new Timing();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new org.apache.curator.retry.RetryOneTime(1));
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new RetryOneTime(1));
			SessionFailRetryLoop retryLoop = client.newSessionFailRetryLoop(SessionFailRetryLoop.Mode.RETRY);
			retryLoop.start();
			try
			{
				client.start();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean secondWasDone = new java.util.concurrent.atomic.AtomicBoolean(false);
				AtomicBoolean secondWasDone = new AtomicBoolean(false);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean firstTime = new java.util.concurrent.atomic.AtomicBoolean(true);
				AtomicBoolean firstTime = new AtomicBoolean(true);
				while (retryLoop.shouldContinue())
				{
					try
					{
						RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper(this, client, firstTime));

						RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper2(this, client, secondWasDone, firstTime));
					}
					catch (Exception e)
					{
						retryLoop.takeException(e);
					}
				}

				Assert.assertTrue(secondWasDone.get());
			}
			finally
			{
				retryLoop.close();
				CloseableUtils.closeQuietly(client);
			}
		}

		private class CallableAnonymousInnerClassHelper : Callable<Void>
		{
			private readonly TestSessionFailRetryLoop outerInstance;

			private org.apache.curator.CuratorZookeeperClient client;
			private AtomicBoolean firstTime;

			public CallableAnonymousInnerClassHelper(TestSessionFailRetryLoop outerInstance, org.apache.curator.CuratorZookeeperClient client, AtomicBoolean firstTime)
			{
				this.outerInstance = outerInstance;
				this.client = client;
				this.firstTime = firstTime;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				if (firstTime.compareAndSet(true, false))
				{
					Assert.assertNull(client.getZooKeeper().exists("/foo/bar", false));
					KillSession.kill(client.getZooKeeper(), outerInstance.server.getConnectString());
					client.getZooKeeper();
					client.blockUntilConnectedOrTimedOut();
				}

				Assert.assertNull(client.getZooKeeper().exists("/foo/bar", false));
				return null;
			}
		}

		private class CallableAnonymousInnerClassHelper2 : Callable<Void>
		{
			private readonly TestSessionFailRetryLoop outerInstance;

			private org.apache.curator.CuratorZookeeperClient client;
			private AtomicBoolean secondWasDone;
			private AtomicBoolean firstTime;

			public CallableAnonymousInnerClassHelper2(TestSessionFailRetryLoop outerInstance, org.apache.curator.CuratorZookeeperClient client, AtomicBoolean secondWasDone, AtomicBoolean firstTime)
			{
				this.outerInstance = outerInstance;
				this.client = client;
				this.secondWasDone = secondWasDone;
				this.firstTime = firstTime;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				Assert.assertFalse(firstTime.get());
				Assert.assertNull(client.getZooKeeper().exists("/foo/bar", false));
				secondWasDone.set(true);
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRetryStatic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRetryStatic()
		{
			Timing timing = new Timing();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new org.apache.curator.retry.RetryOneTime(1));
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new RetryOneTime(1));
			SessionFailRetryLoop retryLoop = client.newSessionFailRetryLoop(SessionFailRetryLoop.Mode.RETRY);
			retryLoop.start();
			try
			{
				client.start();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean secondWasDone = new java.util.concurrent.atomic.AtomicBoolean(false);
				AtomicBoolean secondWasDone = new AtomicBoolean(false);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean firstTime = new java.util.concurrent.atomic.AtomicBoolean(true);
				AtomicBoolean firstTime = new AtomicBoolean(true);
				SessionFailRetryLoop.callWithRetry(client, SessionFailRetryLoop.Mode.RETRY, new CallableAnonymousInnerClassHelper3(this, client, secondWasDone, firstTime));

				Assert.assertTrue(secondWasDone.get());
			}
			finally
			{
				retryLoop.close();
				CloseableUtils.closeQuietly(client);
			}
		}

		private class CallableAnonymousInnerClassHelper3 : Callable<object>
		{
			private readonly TestSessionFailRetryLoop outerInstance;

			private org.apache.curator.CuratorZookeeperClient client;
			private AtomicBoolean secondWasDone;
			private AtomicBoolean firstTime;

			public CallableAnonymousInnerClassHelper3(TestSessionFailRetryLoop outerInstance, org.apache.curator.CuratorZookeeperClient client, AtomicBoolean secondWasDone, AtomicBoolean firstTime)
			{
				this.outerInstance = outerInstance;
				this.client = client;
				this.secondWasDone = secondWasDone;
				this.firstTime = firstTime;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object call() throws Exception
			public override object call()
			{
				RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper4(this));

				RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper5(this));
				return null;
			}

			private class CallableAnonymousInnerClassHelper4 : Callable<Void>
			{
				private readonly CallableAnonymousInnerClassHelper3 outerInstance;

				public CallableAnonymousInnerClassHelper4(CallableAnonymousInnerClassHelper3 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
				public override Void call()
				{
					if (outerInstance.firstTime.compareAndSet(true, false))
					{
						Assert.assertNull(outerInstance.client.getZooKeeper().exists("/foo/bar", false));
						KillSession.kill(outerInstance.client.getZooKeeper(), outerInstance.outerInstance.server.getConnectString());
						outerInstance.client.getZooKeeper();
						outerInstance.client.blockUntilConnectedOrTimedOut();
					}

					Assert.assertNull(outerInstance.client.getZooKeeper().exists("/foo/bar", false));
					return null;
				}
			}

			private class CallableAnonymousInnerClassHelper5 : Callable<Void>
			{
				private readonly CallableAnonymousInnerClassHelper3 outerInstance;

				public CallableAnonymousInnerClassHelper5(CallableAnonymousInnerClassHelper3 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
				public override Void call()
				{
					Assert.assertFalse(outerInstance.firstTime.get());
					Assert.assertNull(outerInstance.client.getZooKeeper().exists("/foo/bar", false));
					outerInstance.secondWasDone.set(true);
					return null;
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBasic()
		{
			Timing timing = new Timing();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new org.apache.curator.retry.RetryOneTime(1));
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new RetryOneTime(1));
			SessionFailRetryLoop retryLoop = client.newSessionFailRetryLoop(SessionFailRetryLoop.Mode.FAIL);
			retryLoop.start();
			try
			{
				client.start();
				try
				{
					while (retryLoop.shouldContinue())
					{
						try
						{
							RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper6(this, client));
						}
						catch (Exception e)
						{
							retryLoop.takeException(e);
						}
					}

					Assert.fail();
				}
				catch (SessionFailRetryLoop.SessionFailedException)
				{
					// correct
				}
			}
			finally
			{
				retryLoop.close();
				CloseableUtils.closeQuietly(client);
			}
		}

		private class CallableAnonymousInnerClassHelper6 : Callable<Void>
		{
			private readonly TestSessionFailRetryLoop outerInstance;

			private org.apache.curator.CuratorZookeeperClient client;

			public CallableAnonymousInnerClassHelper6(TestSessionFailRetryLoop outerInstance, org.apache.curator.CuratorZookeeperClient client)
			{
				this.outerInstance = outerInstance;
				this.client = client;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				Assert.assertNull(client.getZooKeeper().exists("/foo/bar", false));
				KillSession.kill(client.getZooKeeper(), outerInstance.server.getConnectString());

				client.getZooKeeper();
				client.blockUntilConnectedOrTimedOut();
				Assert.assertNull(client.getZooKeeper().exists("/foo/bar", false));
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicStatic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBasicStatic()
		{
			Timing timing = new Timing();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new org.apache.curator.retry.RetryOneTime(1));
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), timing.session(), timing.connection(), null, new RetryOneTime(1));
			SessionFailRetryLoop retryLoop = client.newSessionFailRetryLoop(SessionFailRetryLoop.Mode.FAIL);
			retryLoop.start();
			try
			{
				client.start();
				try
				{
					SessionFailRetryLoop.callWithRetry(client, SessionFailRetryLoop.Mode.FAIL, new CallableAnonymousInnerClassHelper7(this, client));
				}
				catch (SessionFailRetryLoop.SessionFailedException)
				{
					// correct
				}
			}
			finally
			{
				retryLoop.close();
				CloseableUtils.closeQuietly(client);
			}
		}

		private class CallableAnonymousInnerClassHelper7 : Callable<object>
		{
			private readonly TestSessionFailRetryLoop outerInstance;

			private org.apache.curator.CuratorZookeeperClient client;

			public CallableAnonymousInnerClassHelper7(TestSessionFailRetryLoop outerInstance, org.apache.curator.CuratorZookeeperClient client)
			{
				this.outerInstance = outerInstance;
				this.client = client;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Object call() throws Exception
			public override object call()
			{
				RetryLoop.callWithRetry(client, new CallableAnonymousInnerClassHelper8(this));
				return null;
			}

			private class CallableAnonymousInnerClassHelper8 : Callable<Void>
			{
				private readonly CallableAnonymousInnerClassHelper7 outerInstance;

				public CallableAnonymousInnerClassHelper8(CallableAnonymousInnerClassHelper7 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
				public override Void call()
				{
					Assert.assertNull(outerInstance.client.getZooKeeper().exists("/foo/bar", false));
					KillSession.kill(outerInstance.client.getZooKeeper(), outerInstance.outerInstance.server.getConnectString());

					outerInstance.client.getZooKeeper();
					outerInstance.client.blockUntilConnectedOrTimedOut();
					Assert.assertNull(outerInstance.client.getZooKeeper().exists("/foo/bar", false));
					return null;
				}
			}
		}
	}

}