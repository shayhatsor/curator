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

	/// <summary>
	/// Utility to time a method or portion of code
	/// </summary>
	public class TimeTrace
	{
		private readonly string name;
		private readonly TracerDriver driver;
		private readonly long startTimeNanos = System.nanoTime();

		/// <summary>
		/// Create and start a timer
		/// </summary>
		/// <param name="name"> name of the event </param>
		/// <param name="driver"> driver </param>
		public TimeTrace(string name, TracerDriver driver)
		{
			this.name = name;
			this.driver = driver;
		}

		/// <summary>
		/// Record the elapsed time
		/// </summary>
		public virtual void commit()
		{
			long elapsed = System.nanoTime() - startTimeNanos;
			driver.addTrace(name, elapsed, TimeUnit.NANOSECONDS);
		}
	}

}