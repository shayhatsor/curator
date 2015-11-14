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
namespace org.apache.curator
{

	using TracerDriver = org.apache.curator.drivers.TracerDriver;
	using DebugUtils = org.apache.curator.utils.DebugUtils;
	using KeeperException = org.apache.zookeeper.KeeperException;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// <para>Mechanism to perform an operation on Zookeeper that is safe against
	/// disconnections and "recoverable" errors.</para>
	/// 
	/// <para>
	/// If an exception occurs during the operation, the RetryLoop will process it,
	/// check with the current retry policy and either attempt to reconnect or re-throw
	/// the exception
	/// </para>
	/// 
	/// Canonical usage:<br>
	/// <pre>
	/// RetryLoop retryLoop = client.newRetryLoop();
	/// while ( retryLoop.shouldContinue() )
	/// {
	///     try
	///     {
	///         // do your work
	///         ZooKeeper      zk = client.getZooKeeper();    // it's important to re-get the ZK instance in case there was an error and the instance was re-created
	/// 
	///         retryLoop.markComplete();
	///     }
	///     catch ( Exception e )
	///     {
	///         retryLoop.takeException(e);
	///     }
	/// }
	/// </pre>
	/// </summary>
	public class RetryLoop
	{
		private bool isDone = false;
		private int retryCount = 0;

		private readonly Logger log = LoggerFactory.getLogger(this.GetType());
		private readonly long startTimeMs = DateTimeHelperClass.CurrentUnixTimeMillis();
		private readonly RetryPolicy retryPolicy;
		private readonly AtomicReference<TracerDriver> tracer;

		private static readonly RetrySleeper sleeper = new RetrySleeperAnonymousInnerClassHelper();

		private class RetrySleeperAnonymousInnerClassHelper : RetrySleeper
		{
			public RetrySleeperAnonymousInnerClassHelper()
			{
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sleepFor(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException
			public virtual void sleepFor(long time, TimeUnit unit)
			{
				unit.sleep(time);
			}
		}

		/// <summary>
		/// Returns the default retry sleeper
		/// </summary>
		/// <returns> sleeper </returns>
		public static RetrySleeper getDefaultRetrySleeper()
		{
			return sleeper;
		}

		/// <summary>
		/// Convenience utility: creates a retry loop calling the given proc and retrying if needed
		/// </summary>
		/// <param name="client"> Zookeeper </param>
		/// <param name="proc"> procedure to call with retry </param>
		/// @param <T> return type </param>
		/// <returns> procedure result </returns>
		/// <exception cref="Exception"> any non-retriable errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static<T> T callWithRetry(CuratorZookeeperClient client, java.util.concurrent.Callable<T> proc) throws Exception
		public static T callWithRetry<T>(CuratorZookeeperClient client, Callable<T> proc)
		{
			T result = null;
			RetryLoop retryLoop = client.newRetryLoop();
			while (retryLoop.shouldContinue())
			{
				try
				{
					client.internalBlockUntilConnectedOrTimedOut();

					result = proc.call();
					retryLoop.markComplete();
				}
				catch (Exception e)
				{
					retryLoop.takeException(e);
				}
			}
			return result;
		}

		internal RetryLoop(RetryPolicy retryPolicy, AtomicReference<TracerDriver> tracer)
		{
			this.retryPolicy = retryPolicy;
			this.tracer = tracer;
		}

		/// <summary>
		/// If true is returned, make an attempt at the operation
		/// </summary>
		/// <returns> true/false </returns>
		public virtual bool shouldContinue()
		{
			return !isDone;
		}

		/// <summary>
		/// Call this when your operation has successfully completed
		/// </summary>
		public virtual void markComplete()
		{
			isDone = true;
		}

		/// <summary>
		/// Utility - return true if the given Zookeeper result code is retry-able
		/// </summary>
		/// <param name="rc"> result code </param>
		/// <returns> true/false </returns>
		public static bool shouldRetry(int rc)
		{
			return (rc == KeeperException.Code.CONNECTIONLOSS.intValue()) || (rc == KeeperException.Code.OPERATIONTIMEOUT.intValue()) || (rc == KeeperException.Code.SESSIONMOVED.intValue()) || (rc == KeeperException.Code.SESSIONEXPIRED.intValue());
		}

		/// <summary>
		/// Utility - return true if the given exception is retry-able
		/// </summary>
		/// <param name="exception"> exception to check </param>
		/// <returns> true/false </returns>
		public static bool isRetryException(Exception exception)
		{
			if (exception is KeeperException)
			{
				KeeperException keeperException = (KeeperException)exception;
				return shouldRetry(keeperException.code().intValue());
			}
			return false;
		}

		/// <summary>
		/// Pass any caught exceptions here
		/// </summary>
		/// <param name="exception"> the exception </param>
		/// <exception cref="Exception"> if not retry-able or the retry policy returned negative </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void takeException(Exception exception) throws Exception
		public virtual void takeException(Exception exception)
		{
			bool rethrow = true;
			if (isRetryException(exception))
			{
				if (!bool.getBoolean(DebugUtils.PROPERTY_DONT_LOG_CONNECTION_ISSUES))
				{
					log.debug("Retry-able exception received", exception);
				}

				if (retryPolicy.allowRetry(retryCount++, DateTimeHelperClass.CurrentUnixTimeMillis() - startTimeMs, sleeper))
				{
					tracer.get().addCount("retries-allowed", 1);
					if (!bool.getBoolean(DebugUtils.PROPERTY_DONT_LOG_CONNECTION_ISSUES))
					{
						log.debug("Retrying operation");
					}
					rethrow = false;
				}
				else
				{
					tracer.get().addCount("retries-disallowed", 1);
					if (!bool.getBoolean(DebugUtils.PROPERTY_DONT_LOG_CONNECTION_ISSUES))
					{
						log.debug("Retry policy not allowing retry");
					}
				}
			}

			if (rethrow)
			{
				throw exception;
			}
		}
	}

}