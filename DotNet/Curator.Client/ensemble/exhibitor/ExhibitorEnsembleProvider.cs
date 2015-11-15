using System;
using System.Collections.Generic;
using System.Text;
using org.apache.curator.utils;
using org.apache.utils;

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
    /// <summary>
    ///     Ensemble provider that polls a cluster of Exhibitor (https://github.com/Netflix/exhibitor)
    ///     instances for the connection string.
    ///     If the set of instances should change, new ZooKeeper connections will use the new connection
    ///     string.
    /// </summary>
    public class ExhibitorEnsembleProvider : EnsembleProvider
    {
        private const string MIME_TYPE = "application/x-www-form-urlencoded";

        private const string VALUE_PORT = "port";
        private const string VALUE_COUNT = "count";
        private const string VALUE_SERVER_PREFIX = "server";
        private readonly AtomicReference<string> connectionString = new AtomicReference<string>("");
        private readonly AtomicReference<Exhibitors> exhibitors = new AtomicReference<Exhibitors>();
        private static readonly TraceLogger log = TraceLogger.GetLogger(typeof(ExhibitorEnsembleProvider));
        private readonly AtomicReference<Exhibitors> masterExhibitors = new AtomicReference<Exhibitors>();
        private readonly int pollingMs;
        private readonly Random random = new Random();
        private readonly ExhibitorRestClient restClient;
        private readonly string restUriPath;
        private readonly RetryPolicy retryPolicy;

        private readonly ScheduledExecutorService service =
            ThreadUtils.newSingleThreadScheduledExecutor("ExhibitorEnsembleProvider");

        private readonly AtomicReference<State> state = new AtomicReference<State>(State.LATENT);

        /// <param name="exhibitors">
        ///     the current set of exhibitor instances (can be changed later via
        ///     <seealso cref="#setExhibitors(Exhibitors)" />)
        /// </param>
        /// <param name="restClient"> the rest client to use (use <seealso cref="DefaultExhibitorRestClient" /> for most cases) </param>
        /// <param name="restUriPath">
        ///     the path of the REST call used to get the server set. Usually:
        ///     <code>/exhibitor/v1/cluster/list</code>
        /// </param>
        /// <param name="pollingMs"> how ofter to poll the exhibitors for the list </param>
        /// <param name="retryPolicy"> retry policy to use when connecting to the exhibitors </param>
        public ExhibitorEnsembleProvider(Exhibitors exhibitors, ExhibitorRestClient restClient, string restUriPath,
            int pollingMs, RetryPolicy retryPolicy)
        {
            this.exhibitors.set(exhibitors);
            masterExhibitors.set(exhibitors);
            this.restClient = restClient;
            this.restUriPath = restUriPath;
            this.pollingMs = pollingMs;
            this.retryPolicy = retryPolicy;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void start() throws Exception
        public virtual void start()
        {
            Preconditions.checkState(state.compareAndSet(State.LATENT, State.STARTED),
                "Cannot be started more than once");

            service.scheduleWithFixedDelay(() => { poll(); }, pollingMs, pollingMs, TimeUnit.MILLISECONDS);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
        public virtual void close()
        {
            Preconditions.checkState(state.compareAndSet(State.STARTED, State.CLOSED),
                "Already closed or has not been started");

            service.shutdownNow();
        }

        public virtual string getConnectionString()
        {
            return connectionString.get();
        }

        /// <summary>
        ///     Change the set of exhibitors to poll
        /// </summary>
        /// <param name="newExhibitors"> new set </param>
        public virtual void setExhibitors(Exhibitors newExhibitors)
        {
            exhibitors.set(newExhibitors);
            masterExhibitors.set(newExhibitors);
        }

        /// <summary>
        ///     Can be called prior to starting the Curator instance to set the current connection string
        /// </summary>
        /// <exception cref="Exception"> errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pollForInitialEnsemble() throws Exception
        public virtual void pollForInitialEnsemble()
        {
            Preconditions.checkState(state.get() == State.LATENT, "Cannot be called after start()");
            poll();
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting protected void poll()
        protected internal virtual void poll()
        {
            Exhibitors localExhibitors = exhibitors.get();
            var values = queryExhibitors(localExhibitors);

            var count = getCountFromValues(values);
            if (count == 0)
            {
                log.warn("0 count returned from Exhibitors. Using backup connection values.");
                values = useBackup(localExhibitors);
                count = getCountFromValues(values);
            }

            if (count > 0)
            {
                var port = int.Parse(values[VALUE_PORT]);
                var newConnectionString = new StringBuilder();
                IList<string> newHostnames = Lists.newArrayList();

                for (var i = 0; i < count; ++i)
                {
                    if (newConnectionString.Length > 0)
                    {
                        newConnectionString.Append(",");
                    }
                    var server = values[VALUE_SERVER_PREFIX + i];
                    newConnectionString.Append(server).Append(":").Append(port);
                    newHostnames.Add(server);
                }

                var newConnectionStringValue = newConnectionString.ToString();
                if (!newConnectionStringValue.Equals(connectionString.get()))
                {
                    log.info(string.Format("Connection string has changed. Old value ({0}), new value ({1})",
                        connectionString.get(), newConnectionStringValue));
                }
                var newExhibitors = new Exhibitors(newHostnames, localExhibitors.getRestPort(),
                    new BackupConnectionStringProviderAnonymousInnerClassHelper(this));
                connectionString.set(newConnectionStringValue);
                exhibitors.set(newExhibitors);
            }
        }

        private int getCountFromValues(IDictionary<string, string> values)
        {
            try
            {
                return int.Parse(values[VALUE_COUNT]);
            }
            catch (FormatException)
            {
                // ignore
            }
            return 0;
        }

        private IDictionary<string, string> useBackup(Exhibitors localExhibitors)
        {
            var values = newValues();

            try
            {
                var backupConnectionString = localExhibitors.getBackupConnectionString();

                var thePort = -1;
                var count = 0;
                foreach (var spec in backupConnectionString.Split(",", true))
                {
                    spec = spec.Trim();
                    var parts = spec.Split(":", true);
                    if (parts.Length == 2)
                    {
                        var hostname = parts[0];
                        var port = int.Parse(parts[1]);
                        if (thePort < 0)
                        {
                            thePort = port;
                        }
                        else if (port != thePort)
                        {
                            log.warn("Inconsistent port in connection component: " + spec);
                        }
                        values[VALUE_SERVER_PREFIX + count] = hostname;
                        ++count;
                    }
                    else
                    {
                        log.warn("Bad backup connection component: " + spec);
                    }
                }
                values[VALUE_COUNT] = Convert.ToString(count);
                values[VALUE_PORT] = Convert.ToString(thePort);
            }
            catch (Exception e)
            {
                log.error("Couldn't get backup connection string", e);
            }
            return values;
        }

        private IDictionary<string, string> newValues()
        {
            IDictionary<string, string> values = Maps.newHashMap();
            values[VALUE_COUNT] = "0";
            return values;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.Map<String, String> decodeExhibitorList(String str) throws java.io.UnsupportedEncodingException
        private static IDictionary<string, string> decodeExhibitorList(string str)
        {
            IDictionary<string, string> values = Maps.newHashMap();
            foreach (var spec in str.Split("&", true))
            {
                var parts = spec.Split("=", true);
                if (parts.Length == 2)
                {
                    values[parts[0]] = URLDecoder.decode(parts[1], "UTF-8");
                }
            }

            return values;
        }

        private IDictionary<string, string> queryExhibitors(Exhibitors localExhibitors)
        {
            var values = newValues();

            var start = TimeHelper.ElapsedMiliseconds;
            var retries = 0;
            var done = false;
            while (!done)
            {
                IList<string> hostnames = Lists.newArrayList(localExhibitors.getHostnames());
                if (hostnames.Count == 0)
                {
                    done = true;
                }
                else
                {
                    var hostname = hostnames[random.Next(hostnames.Count)];
                    try
                    {
                        var encoded = restClient.getRaw(hostname, localExhibitors.getRestPort(), restUriPath, MIME_TYPE);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
                        values.putAll(decodeExhibitorList(encoded));
                        done = true;
                    }
                    catch (Exception e)
                    {
                        if (retryPolicy.allowRetry(retries++, TimeHelper.ElapsedMiliseconds - start,
                            RetryLoop.getDefaultRetrySleeper()))
                        {
                            log.warn("Couldn't get servers from Exhibitor. Retrying.", e);
                        }
                        else
                        {
                            log.error("Couldn't get servers from Exhibitor. Giving up.", e);
                            done = true;
                        }
                    }
                }
            }

            return values;
        }

        private enum State
        {
            LATENT,
            STARTED,
            CLOSED
        }

        private class BackupConnectionStringProviderAnonymousInnerClassHelper :
            Exhibitors.BackupConnectionStringProvider
        {
            private readonly ExhibitorEnsembleProvider outerInstance;

            public BackupConnectionStringProviderAnonymousInnerClassHelper(ExhibitorEnsembleProvider outerInstance)
            {
                this.outerInstance = outerInstance;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getBackupConnectionString() throws Exception
            public virtual string getBackupConnectionString()
            {
                return outerInstance.masterExhibitors.get().getBackupConnectionString();
                    // this may be overloaded by clients. Make sure there is always a method call to get the value.
            }
        }
    }
}