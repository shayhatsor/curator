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


	using Assert = org.testng.Assert;
	using AfterMethod = org.testng.annotations.AfterMethod;
	using BeforeMethod = org.testng.annotations.BeforeMethod;
	using Test = org.testng.annotations.Test;

	public class TestCloseableScheduledExecutorService
	{
		private const int QTY = 10;
		private const int DELAY_MS = 100;

		private volatile ScheduledExecutorService executorService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeMethod public void setup()
		public virtual void setup()
		{
			executorService = Executors.newScheduledThreadPool(QTY * 2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterMethod public void tearDown()
		public virtual void tearDown()
		{
			executorService.shutdownNow();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCloseableScheduleWithFixedDelay() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCloseableScheduleWithFixedDelay()
		{
			CloseableScheduledExecutorService service = new CloseableScheduledExecutorService(executorService);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(QTY);
			CountDownLatch latch = new CountDownLatch(QTY);
			service.scheduleWithFixedDelay(() =>
					{
						latch.countDown();
					}, DELAY_MS, DELAY_MS, TimeUnit.MILLISECONDS);

			Assert.assertTrue(latch.@await((QTY * 2) * DELAY_MS, TimeUnit.MILLISECONDS));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCloseableScheduleWithFixedDelayAndAdditionalTasks() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void testCloseableScheduleWithFixedDelayAndAdditionalTasks()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger outerCounter = new java.util.concurrent.atomic.AtomicInteger(0);
			AtomicInteger outerCounter = new AtomicInteger(0);
			Runnable command = () =>
			{
				outerCounter.incrementAndGet();
			};
			executorService.scheduleWithFixedDelay(command, DELAY_MS, DELAY_MS, TimeUnit.MILLISECONDS);

			CloseableScheduledExecutorService service = new CloseableScheduledExecutorService(executorService);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger innerCounter = new java.util.concurrent.atomic.AtomicInteger(0);
			AtomicInteger innerCounter = new AtomicInteger(0);
			service.scheduleWithFixedDelay(() =>
			{
				innerCounter.incrementAndGet();
			}, DELAY_MS, DELAY_MS, TimeUnit.MILLISECONDS);

			Thread.Sleep(DELAY_MS * 4);

			service.close();
			Thread.Sleep(DELAY_MS * 2);

			int innerValue = innerCounter.get();
			Assert.assertTrue(innerValue > 0);

			int value = outerCounter.get();
			Thread.Sleep(DELAY_MS * 2);
			int newValue = outerCounter.get();
			Assert.assertTrue(newValue > value);
			Assert.assertEquals(innerValue, innerCounter.get());

			value = newValue;
			Thread.Sleep(DELAY_MS * 2);
			newValue = outerCounter.get();
			Assert.assertTrue(newValue > value);
			Assert.assertEquals(innerValue, innerCounter.get());
		}
	}

}