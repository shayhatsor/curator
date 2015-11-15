using System;
using System.Collections.Generic;
using System.Threading;

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

	using Lists = com.google.common.collect.Lists;
	using Assert = org.testng.Assert;
	using AfterMethod = org.testng.annotations.AfterMethod;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	public class TestCloseableExecutorService
	{
		private const int QTY = 10;

		private volatile ExecutorService executorService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			executorService = Executors.newFixedThreadPool(QTY * 2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterMethod public void tearDown()
		public virtual void tearDown()
		{
			executorService.shutdownNow();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicRunnable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBasicRunnable()
		{
			try
			{
				CloseableExecutorService service = new CloseableExecutorService(executorService);
				CountDownLatch startLatch = new CountDownLatch(QTY);
				CountDownLatch latch = new CountDownLatch(QTY);
				for (int i = 0; i < QTY; ++i)
				{
					submitRunnable(service, startLatch, latch);
				}

				Assert.assertTrue(startLatch.@await(3, TimeUnit.SECONDS));
				service.close();
				Assert.assertTrue(latch.@await(3, TimeUnit.SECONDS));
			}
			catch (AssertionError e)
			{
				throw e;
			}
			catch (Exception e)
			{
				e.printStackTrace();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicCallable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testBasicCallable()
		{
			CloseableExecutorService service = new CloseableExecutorService(executorService);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startLatch = new java.util.concurrent.CountDownLatch(QTY);
			CountDownLatch startLatch = new CountDownLatch(QTY);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(QTY);
			CountDownLatch latch = new CountDownLatch(QTY);
			for (int i = 0; i < QTY; ++i)
			{
				service.submit(new CallableAnonymousInnerClassHelper(this, startLatch, latch));
			}

			Assert.assertTrue(startLatch.@await(3, TimeUnit.SECONDS));
			service.close();
			Assert.assertTrue(latch.@await(3, TimeUnit.SECONDS));
		}

		private class CallableAnonymousInnerClassHelper : Callable<Void>
		{
			private readonly TestCloseableExecutorService outerInstance;

			private CountDownLatch startLatch;
			private CountDownLatch latch;

			public CallableAnonymousInnerClassHelper(TestCloseableExecutorService outerInstance, CountDownLatch startLatch, CountDownLatch latch)
			{
				this.outerInstance = outerInstance;
				this.startLatch = startLatch;
				this.latch = latch;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				try
				{
					startLatch.countDown();
					Thread.CurrentThread.Join();
				}
				catch (InterruptedException)
				{
					Thread.CurrentThread.Interrupt();
				}
				finally
				{
					latch.countDown();
				}
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testListeningRunnable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testListeningRunnable()
		{
			CloseableExecutorService service = new CloseableExecutorService(executorService);
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = com.google.common.collect.Lists.newArrayList();
			IList<Future<?>> futures = Lists.newArrayList();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startLatch = new java.util.concurrent.CountDownLatch(QTY);
			CountDownLatch startLatch = new CountDownLatch(QTY);
			for (int i = 0; i < QTY; ++i)
			{
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = service.submit(new Runnable()
				Future<?> future = service.submit(() =>
					{
						try
						{
							startLatch.countDown();
							Thread.CurrentThread.Join();
						}
						catch (InterruptedException)
						{
							Thread.CurrentThread.Interrupt();
						}
					});
				futures.Add(future);
			}

			Assert.assertTrue(startLatch.@await(3, TimeUnit.SECONDS));

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			foreach (Future<?> future in futures)
			{
				future.cancel(true);
			}

			Assert.assertEquals(service.size(), 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testListeningCallable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testListeningCallable()
		{
			CloseableExecutorService service = new CloseableExecutorService(executorService);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startLatch = new java.util.concurrent.CountDownLatch(QTY);
			CountDownLatch startLatch = new CountDownLatch(QTY);
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = com.google.common.collect.Lists.newArrayList();
			IList<Future<?>> futures = Lists.newArrayList();
			for (int i = 0; i < QTY; ++i)
			{
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = service.submit(new java.util.concurrent.Callable<Void>()
				Future<?> future = service.submit(new CallableAnonymousInnerClassHelper2(this, startLatch));
				futures.Add(future);
			}

			Assert.assertTrue(startLatch.@await(3, TimeUnit.SECONDS));
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			foreach (Future<?> future in futures)
			{
				future.cancel(true);
			}

			Assert.assertEquals(service.size(), 0);
		}

		private class CallableAnonymousInnerClassHelper2 : Callable<Void>
		{
			private readonly TestCloseableExecutorService outerInstance;

			private CountDownLatch startLatch;

			public CallableAnonymousInnerClassHelper2(TestCloseableExecutorService outerInstance, CountDownLatch startLatch)
			{
				this.outerInstance = outerInstance;
				this.startLatch = startLatch;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Void call() throws Exception
			public override Void call()
			{
				try
				{
					startLatch.countDown();
					Thread.CurrentThread.Join();
				}
				catch (InterruptedException)
				{
					Thread.CurrentThread.Interrupt();
				}
				return null;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPartialRunnable() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testPartialRunnable()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch outsideLatch = new java.util.concurrent.CountDownLatch(1);
			CountDownLatch outsideLatch = new CountDownLatch(1);
			executorService.submit(() =>
				{
					try
					{
						Thread.CurrentThread.Join();
					}
					catch (InterruptedException)
					{
						Thread.CurrentThread.Interrupt();
					}
					finally
					{
						outsideLatch.countDown();
					}
				});

			CloseableExecutorService service = new CloseableExecutorService(executorService);
			CountDownLatch startLatch = new CountDownLatch(QTY);
			CountDownLatch latch = new CountDownLatch(QTY);
			for (int i = 0; i < QTY; ++i)
			{
				submitRunnable(service, startLatch, latch);
			}

			while (service.size() < QTY)
			{
				Thread.Sleep(100);
			}

			Assert.assertTrue(startLatch.@await(3, TimeUnit.SECONDS));
			service.close();
			Assert.assertTrue(latch.@await(3, TimeUnit.SECONDS));
			Assert.assertEquals(outsideLatch.getCount(), 1);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void submitRunnable(CloseableExecutorService service, final java.util.concurrent.CountDownLatch startLatch, final java.util.concurrent.CountDownLatch latch)
		private void submitRunnable(CloseableExecutorService service, CountDownLatch startLatch, CountDownLatch latch)
		{
			service.submit(() =>
					{
						try
						{
							startLatch.countDown();
							Thread.Sleep(100000);
						}
						catch (InterruptedException)
						{
							Thread.CurrentThread.Interrupt();
						}
						finally
						{
							latch.countDown();
						}
					});
		}
	}

}