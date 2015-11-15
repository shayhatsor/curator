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

namespace org.apache.curator.ensemble.@fixed
{
    using Preconditions = com.google.common.@base.Preconditions;

    /// <summary>
    ///     Standard ensemble provider that wraps a fixed connection string
    /// </summary>
    public class FixedEnsembleProvider : EnsembleProvider
    {
        private readonly string connectionString;

        /// <summary>
        ///     The connection string to use
        /// </summary>
        /// <param name="connectionString"> connection string </param>
        public FixedEnsembleProvider(string connectionString)
        {
            this.connectionString = Preconditions.checkNotNull(connectionString, "connectionString cannot be null");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void start() throws Exception
        public virtual void start()
        {
            // NOP
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
        public virtual void close()
        {
            // NOP
        }

        public virtual string getConnectionString()
        {
            return connectionString;
        }
    }
}