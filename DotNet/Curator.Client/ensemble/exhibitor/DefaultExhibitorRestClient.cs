using System.IO;
using System.Text;
using org.apache.curator.utils;

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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("UnusedDeclaration") public class DefaultExhibitorRestClient implements ExhibitorRestClient
    public class DefaultExhibitorRestClient : ExhibitorRestClient
    {
        private readonly bool useSsl;

        public DefaultExhibitorRestClient() : this(false)
        {
        }

        public DefaultExhibitorRestClient(bool useSsl)
        {
            this.useSsl = useSsl;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public String getRaw(String hostname, int port, String uriPath, String mimeType) throws Exception
        public virtual string getRaw(string hostname, int port, string uriPath, string mimeType)
        {
            URI uri = new URI(useSsl ? "https" : "http", null, hostname, port, uriPath, null, null);
            HttpURLConnection connection = (HttpURLConnection) uri.toURL().openConnection();
            connection.addRequestProperty("Accept", mimeType);
            var str = new StringBuilder();
            Stream @in = new BufferedInputStream(connection.getInputStream());
            try
            {
                for (;;)
                {
                    var b = @in.Read();
                    if (b < 0)
                    {
                        break;
                    }
                    str.Append((char) (b & 0xff));
                }
            }
            finally
            {
                CloseableUtils.closeQuietly(@in);
            }
            return str.ToString();
        }
    }
}