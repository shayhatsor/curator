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


using System;
using System.IO;

namespace org.apache.curator.utils
{
    /// <summary>
    ///     This class adds back functionality that was removed in Guava v16.0.
    /// </summary>
    public class CloseableUtils
    {
        private static readonly Logger log = LoggerFactory.getLogger(typeof (CloseableUtils));

        /// <summary>
        ///     <para>
        ///         This method has been added because Guava has removed the
        ///         {@code closeQuietly()} method from {@code Closeables} in v16.0. It's
        ///         tempting simply to replace calls to {@code closeQuietly(closeable)}
        ///         with calls to {@code close(closeable, true)} to close
        ///         {@code Closeable}s while swallowing {@code IOException}s, but
        ///         {@code close()} is declared as {@code throws IOException} whereas
        ///         {@code closeQuietly()} is not, so it's not a drop-in replacement.
        ///     </para>
        ///     <para>
        ///         On the whole, Guava is very backwards compatible. By fixing this nit,
        ///         Curator can continue to support newer versions of Guava without having
        ///         to bump its own dependency version.
        ///     </para>
        ///     <para>
        ///         See
        ///         <a href="https://issues.apache.org/jira/browse/CURATOR-85">https://issues.apache.org/jira/browse/CURATOR-85</a>
        ///     </para>
        /// </summary>
        public static void closeQuietly(IDisposable closeable)
        {
            try
            {
                // Here we've instructed Guava to swallow the IOException
                Closeables.close(closeable, true);
            }
            catch (IOException e)
            {
                // We instructed Guava to swallow the IOException, so this should
                // never happen. Since it did, log it.
                log.error("IOException should not have been thrown.", e);
            }
        }
    }
}