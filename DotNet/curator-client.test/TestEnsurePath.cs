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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.@when;


	using RetryOneTime = org.apache.curator.retry.RetryOneTime;
	using EnsurePath = org.apache.curator.utils.EnsurePath;
	using ZooKeeper = org.apache.zookeeper.ZooKeeper;
	using Stat = org.apache.zookeeper.data.Stat;
	using Mockito = org.mockito.Mockito;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;
	using Assert = org.testng.Assert;
	using Test = org.testng.annotations.Test;

	public class TestEnsurePath
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBasic()
		{
			ZooKeeper client = mock(typeof(ZooKeeper), Mockito.RETURNS_MOCKS);
			CuratorZookeeperClient curator = mock(typeof(CuratorZookeeperClient));
			RetryPolicy retryPolicy = new RetryOneTime(1);
			RetryLoop retryLoop = new RetryLoop(retryPolicy, null);
			@when(curator.getZooKeeper()).thenReturn(client);
			@when(curator.getRetryPolicy()).thenReturn(retryPolicy);
			@when(curator.newRetryLoop()).thenReturn(retryLoop);

			Stat fakeStat = mock(typeof(Stat));
			@when(client.exists(Mockito.any<string>(), anyBoolean())).thenReturn(fakeStat);

			EnsurePath ensurePath = new EnsurePath("/one/two/three");
			ensurePath.ensure(curator);

			verify(client, times(3)).exists(Mockito.any<string>(), anyBoolean());

			ensurePath.ensure(curator);
			verifyNoMoreInteractions(client);
			ensurePath.ensure(curator);
			verifyNoMoreInteractions(client);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimultaneous() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testSimultaneous()
		{
			ZooKeeper client = mock(typeof(ZooKeeper), Mockito.RETURNS_MOCKS);
			RetryPolicy retryPolicy = new RetryOneTime(1);
			RetryLoop retryLoop = new RetryLoop(retryPolicy, null);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CuratorZookeeperClient curator = mock(CuratorZookeeperClient.class);
			CuratorZookeeperClient curator = mock(typeof(CuratorZookeeperClient));
			@when(curator.getZooKeeper()).thenReturn(client);
			@when(curator.getRetryPolicy()).thenReturn(retryPolicy);
			@when(curator.newRetryLoop()).thenReturn(retryLoop);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.zookeeper.data.Stat fakeStat = mock(org.apache.zookeeper.data.Stat.class);
			Stat fakeStat = mock(typeof(Stat));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startedLatch = new java.util.concurrent.CountDownLatch(2);
			CountDownLatch startedLatch = new CountDownLatch(2);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch finishedLatch = new java.util.concurrent.CountDownLatch(2);
			CountDownLatch finishedLatch = new CountDownLatch(2);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore semaphore = new java.util.concurrent.Semaphore(0);
			Semaphore semaphore = new Semaphore(0);
			@when(client.exists(Mockito.any<string>(), anyBoolean())).thenAnswer(new AnswerAnonymousInnerClassHelper(this, fakeStat, semaphore));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.curator.utils.EnsurePath ensurePath = new org.apache.curator.utils.EnsurePath("/one/two/three");
			EnsurePath ensurePath = new EnsurePath("/one/two/three");
			ExecutorService service = Executors.newCachedThreadPool();
			for (int i = 0; i < 2; ++i)
			{
				service.submit(new CallableAnonymousInnerClassHelper(this, curator, startedLatch, finishedLatch, ensurePath));
			}

			Assert.assertTrue(startedLatch.@await(10, TimeUnit.SECONDS));
			semaphore.release(3);
			Assert.assertTrue(finishedLatch.@await(10, TimeUnit.SECONDS));
			verify(client, times(3)).exists(Mockito.any<string>(), anyBoolean());

			ensurePath.ensure(curator);
			verifyNoMoreInteractions(client);
			ensurePath.ensure(curator);
			verifyNoMoreInteractions(client);
		}

		private class AnswerAnonymousInnerClassHelper : Answer<Stat>
		{
			private readonly TestEnsurePath outerInstance;

			private Stat fakeStat;
			private Semaphore semaphore;

			public AnswerAnonymousInnerClassHelper(TestEnsurePath outerInstance, Stat fakeStat, Semaphore semaphore)
			{
				this.outerInstance = outerInstance;
				this.fakeStat = fakeStat;
				this.semaphore = semaphore;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public org.apache.zookeeper.data.Stat answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override Stat answer(InvocationOnMock invocation)
			{
				semaphore.acquire();
				return fakeStat;
			}
		}

		private class CallableAnonymousInnerClassHelper : Callable<Void>
		{
			private readonly TestEnsurePath outerInstance;

			private org.apache.curator.CuratorZookeeperClient curator;
			private CountDownLatch startedLatch;
			private CountDownLatch finishedLatch;
			private EnsurePath ensurePath;

			public CallableAnonymousInnerClassHelper(TestEnsurePath outerInstance, org.apache.curator.CuratorZookeeperClient curator, CountDownLatch startedLatch, CountDownLatch finishedLatch, EnsurePath ensurePath)
			{
				this.outerInstance = outerInstance;
				this.curator = curator;
				this.startedLatch = startedLatch;
				this.finishedLatch = finishedLatch;
				this.ensurePath = ensurePath;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				startedLatch.countDown();
				ensurePath.ensure(curator);
				finishedLatch.countDown();
				return null;
			}
		}
	}

}