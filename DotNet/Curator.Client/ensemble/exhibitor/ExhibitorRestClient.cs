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

namespace org.apache.curator.ensemble.exhibitor
{
    public interface ExhibitorRestClient
    {
        /// <summary>
        ///     Connect to the given Exhibitor and return the raw result
        /// </summary>
        /// <param name="hostname"> host to connect to </param>
        /// <param name="port"> connect port </param>
        /// <param name="uriPath"> path </param>
        /// <param name="mimeType"> Accept mime type </param>
        /// <returns> raw result </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getRaw(String hostname, int port, String uriPath, String mimeType) throws Exception;
        string getRaw(string hostname, int port, string uriPath, string mimeType);
    }
}