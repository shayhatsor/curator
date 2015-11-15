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

	using ExponentialBackoffRetry = org.apache.curator.retry.ExponentialBackoffRetry;
	using RetryForever = org.apache.curator.retry.RetryForever;
	using RetryOneTime = org.apache.curator.retry.RetryOneTime;
	using BaseClassForTests = org.apache.curator.test.BaseClassForTests;
	using CreateMode = org.apache.zookeeper.CreateMode;
	using ZooDefs = org.apache.zookeeper.ZooDefs;
	using Mockito = org.mockito.Mockito;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;

	public class TestRetryLoop : BaseClassForTests
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExponentialBackoffRetryLimit()
		public virtual void testExponentialBackoffRetryLimit()
		{
			RetrySleeper sleeper = new RetrySleeperAnonymousInnerClassHelper(this);
			ExponentialBackoffRetry retry = new ExponentialBackoffRetry(1, int.MaxValue, 100);
			for (int i = 0; i >= 0; ++i)
			{
				retry.allowRetry(i, 0, sleeper);
			}
		}

		private class RetrySleeperAnonymousInnerClassHelper : RetrySleeper
		{
			private readonly TestRetryLoop outerInstance;

			public RetrySleeperAnonymousInnerClassHelper(TestRetryLoop outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sleepFor(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException
			public virtual void sleepFor(long time, TimeUnit unit)
			{
				Assert.assertTrue(unit.toMillis(time) <= 100);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRetryLoopWithFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRetryLoopWithFailure()
		{
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), 5000, 5000, null, new RetryOneTime(1));
			client.start();
			try
			{
				int loopCount = 0;
				RetryLoop retryLoop = client.newRetryLoop();
				while (retryLoop.shouldContinue())
				{
					++loopCount;
					switch (loopCount)
					{
						case 1:
						{
							server.stop();
							break;
						}

						case 2:
						{
							server.restart();
							break;
						}

						case 3:
						case 4:
						{
							// ignore
							break;
						}

						default:
						{
							Assert.fail();
							goto outerBreak;
						}
					}

					try
					{
						client.blockUntilConnectedOrTimedOut();
						client.getZooKeeper().create("/test", new sbyte[]{1,2,3}, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
						retryLoop.markComplete();
					}
					catch (Exception e)
					{
						retryLoop.takeException(e);
					}
					outerContinue:;
				}
				outerBreak:

				Assert.assertTrue(loopCount >= 2);
			}
			finally
			{
				client.close();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRetryLoop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRetryLoop()
		{
			CuratorZookeeperClient client = new CuratorZookeeperClient(server.getConnectString(), 10000, 10000, null, new RetryOneTime(1));
			client.start();
			try
			{
				int loopCount = 0;
				RetryLoop retryLoop = client.newRetryLoop();
				while (retryLoop.shouldContinue())
				{
					if (++loopCount > 2)
					{
						Assert.fail();
						break;
					}

					try
					{
						client.getZooKeeper().create("/test", new sbyte[]{1,2,3}, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
						retryLoop.markComplete();
					}
					catch (Exception e)
					{
						retryLoop.takeException(e);
					}
				}

				Assert.assertTrue(loopCount > 0);
			}
			finally
			{
				client.close();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRetryForever() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testRetryForever()
		{
			int retryIntervalMs = 1;
			RetrySleeper sleeper = Mockito.mock(typeof(RetrySleeper));
			RetryForever retryForever = new RetryForever(retryIntervalMs);

			for (int i = 0; i < 10; i++)
			{
				bool allowed = retryForever.allowRetry(i, 0, sleeper);
				Assert.assertTrue(allowed);
				Mockito.verify(sleeper, times(i + 1)).sleepFor(retryIntervalMs, TimeUnit.MILLISECONDS);
			}
		}
	}

}