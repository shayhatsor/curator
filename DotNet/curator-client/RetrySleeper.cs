﻿/// <summary>
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

	/// <summary>
	/// Abstraction for retry policies to sleep
	/// </summary>
	public interface RetrySleeper
	{
		/// <summary>
		/// Sleep for the given time
		/// </summary>
		/// <param name="time"> time </param>
		/// <param name="unit"> time unit </param>
		/// <exception cref="InterruptedException"> if the sleep is interrupted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sleepFor(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException;
		void sleepFor(long time, TimeUnit unit);
	}

}