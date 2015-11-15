using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using org.apache.utils;
using org.apache.zookeeper;
using org.apache.zookeeper.data;

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
    public class ZKPaths
    {
        /// <summary>
        ///     Zookeeper's path separator character.
        /// </summary>
        public const string PATH_SEPARATOR = "/";

        private static readonly CreateMode NON_CONTAINER_MODE = CreateMode.PERSISTENT;

        private static readonly Splitter PATH_SPLITTER = Splitter.on(PATH_SEPARATOR).omitEmptyStrings();

        private ZKPaths()
        {
        }

        /// <returns>
        ///     <seealso cref="CreateMode#CONTAINER" /> if the ZK JAR supports it. Otherwise
        ///     <seealso cref="CreateMode#PERSISTENT" />
        /// </returns>
        public static CreateMode getContainerCreateMode()
        {
            return CreateModeHolder.containerCreateMode;
        }

        /// <summary>
        ///     Returns true if the version of ZooKeeper client in use supports containers
        /// </summary>
        /// <returns> true/false </returns>
        public static bool hasContainerSupport()
        {
            return getContainerCreateMode() != NON_CONTAINER_MODE;
        }

        /// <summary>
        ///     Apply the namespace to the given path
        /// </summary>
        /// <param name="namespace"> namespace (can be null) </param>
        /// <param name="path">      path </param>
        /// <returns> adjusted path </returns>
        public static string fixForNamespace(string @namespace, string path)
        {
            return fixForNamespace(@namespace, path, false);
        }

        /// <summary>
        ///     Apply the namespace to the given path
        /// </summary>
        /// <param name="namespace">    namespace (can be null) </param>
        /// <param name="path">         path </param>
        /// <param name="isSequential"> if the path is being created with a sequential flag </param>
        /// <returns> adjusted path </returns>
        public static string fixForNamespace(string @namespace, string path, bool isSequential)
        {
            // Child path must be valid in and of itself.
            PathUtils.validatePath(path, isSequential);

            if (@namespace != null)
            {
                return makePath(@namespace, path);
            }
            return path;
        }

        /// <summary>
        ///     Given a full path, return the node name. i.e. "/one/two/three" will return "three"
        /// </summary>
        /// <param name="path"> the path </param>
        /// <returns> the node </returns>
        public static string getNodeFromPath(string path)
        {
            PathUtils.validatePath(path);
            var i = path.LastIndexOf(PATH_SEPARATOR, StringComparison.Ordinal);
            if (i < 0)
            {
                return path;
            }
            if (i + 1 >= path.Length)
            {
                return "";
            }
            return path.Substring(i + 1);
        }

        /// <summary>
        ///     Given a full path, return the node name and its path. i.e. "/one/two/three" will return {"/one/two", "three"}
        /// </summary>
        /// <param name="path"> the path </param>
        /// <returns> the node </returns>
        public static PathAndNode getPathAndNode(string path)
        {
            PathUtils.validatePath(path);
            var i = path.LastIndexOf(PATH_SEPARATOR, StringComparison.Ordinal);
            if (i < 0)
            {
                return new PathAndNode(path, "");
            }
            if (i + 1 >= path.Length)
            {
                return new PathAndNode(PATH_SEPARATOR, "");
            }
            var node = path.Substring(i + 1);
            var parentPath = i > 0 ? path.Substring(0, i) : PATH_SEPARATOR;
            return new PathAndNode(parentPath, node);
        }

        /// <summary>
        ///     Given a full path, return the the individual parts, without slashes.
        ///     The root path will return an empty list.
        /// </summary>
        /// <param name="path"> the path </param>
        /// <returns> an array of parts </returns>
        public static IList<string> split(string path)
        {
            PathUtils.validatePath(path);
            return PATH_SPLITTER.splitToList(path);
        }

        /// <summary>
        ///     Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
        ///     between directories and files. So, every node in the path is created. The data for each node is an empty blob
        /// </summary>
        /// <param name="zookeeper"> the client </param>
        /// <param name="path">      path to ensure </param>
        /// <exception cref="InterruptedException">                 thread interruption </exception>
        /// <exception cref="org.apache.zookeeper.KeeperException"> Zookeeper errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void mkdirs(org.apache.zookeeper.ZooKeeper zookeeper, String path) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static void mkdirs(ZooKeeper zookeeper, string path)
        {
            mkdirs(zookeeper, path, true, null, false);
        }

        /// <summary>
        ///     Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
        ///     between directories and files. So, every node in the path is created. The data for each node is an empty blob
        /// </summary>
        /// <param name="zookeeper">    the client </param>
        /// <param name="path">         path to ensure </param>
        /// <param name="makeLastNode"> if true, all nodes are created. If false, only the parent nodes are created </param>
        /// <exception cref="InterruptedException">                 thread interruption </exception>
        /// <exception cref="org.apache.zookeeper.KeeperException"> Zookeeper errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void mkdirs(org.apache.zookeeper.ZooKeeper zookeeper, String path, boolean makeLastNode) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static void mkdirs(ZooKeeper zookeeper, string path, bool makeLastNode)
        {
            mkdirs(zookeeper, path, makeLastNode, null, false);
        }

        /// <summary>
        ///     Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
        ///     between directories and files. So, every node in the path is created. The data for each node is an empty blob
        /// </summary>
        /// <param name="zookeeper">    the client </param>
        /// <param name="path">         path to ensure </param>
        /// <param name="makeLastNode"> if true, all nodes are created. If false, only the parent nodes are created </param>
        /// <param name="aclProvider">  if not null, the ACL provider to use when creating parent nodes </param>
        /// <exception cref="InterruptedException">                 thread interruption </exception>
        /// <exception cref="org.apache.zookeeper.KeeperException"> Zookeeper errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void mkdirs(org.apache.zookeeper.ZooKeeper zookeeper, String path, boolean makeLastNode, InternalACLProvider aclProvider) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static void mkdirs(ZooKeeper zookeeper, string path, bool makeLastNode, InternalACLProvider aclProvider)
        {
            mkdirs(zookeeper, path, makeLastNode, aclProvider, false);
        }

        /// <summary>
        ///     Make sure all the nodes in the path are created. NOTE: Unlike File.mkdirs(), Zookeeper doesn't distinguish
        ///     between directories and files. So, every node in the path is created. The data for each node is an empty blob
        /// </summary>
        /// <param name="zookeeper">    the client </param>
        /// <param name="path">         path to ensure </param>
        /// <param name="makeLastNode"> if true, all nodes are created. If false, only the parent nodes are created </param>
        /// <param name="aclProvider">  if not null, the ACL provider to use when creating parent nodes </param>
        /// <param name="asContainers"> if true, nodes are created as <seealso cref="CreateMode#CONTAINER" /> </param>
        /// <exception cref="InterruptedException">                 thread interruption </exception>
        /// <exception cref="org.apache.zookeeper.KeeperException"> Zookeeper errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void mkdirs(org.apache.zookeeper.ZooKeeper zookeeper, String path, boolean makeLastNode, InternalACLProvider aclProvider, boolean asContainers) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static async void mkdirs(ZooKeeper zookeeper, string path, bool makeLastNode, InternalACLProvider aclProvider,
            bool asContainers)
        {
            PathUtils.validatePath(path);

            var pos = 1; // skip first slash, root is guaranteed to exist
            do
            {
                pos = path.IndexOf(PATH_SEPARATOR, pos + 1, StringComparison.Ordinal);

                if (pos == -1)
                {
                    if (makeLastNode)
                    {
                        pos = path.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                var subPath = path.Substring(0, pos);
                if ((await zookeeper.existsAsync(subPath, false) )== null)
                {
                    try
                    {
                        List<ACL> acl = null;
                        if (aclProvider != null)
                        {
                            acl = aclProvider.getAclForPath(path);
                            if (acl == null)
                            {
                                acl = aclProvider.getDefaultAcl();
                            }
                        }
                        if (acl == null)
                        {
                            acl = ZooDefs.Ids.OPEN_ACL_UNSAFE;
                        }
                        await zookeeper.createAsync(subPath, new byte[0], acl, getCreateMode(asContainers));
                    }
                    catch (KeeperException.NodeExistsException)
                    {
                        // ignore... someone else has created it since we checked
                    }
                }
            } while (pos < path.Length);
        }

        /// <summary>
        ///     Recursively deletes children of a node.
        /// </summary>
        /// <param name="zookeeper">  the client </param>
        /// <param name="path">       path of the node to delete </param>
        /// <param name="deleteSelf"> flag that indicates that the node should also get deleted </param>
        /// <exception cref="InterruptedException"> </exception>
        /// <exception cref="KeeperException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void deleteChildren(org.apache.zookeeper.ZooKeeper zookeeper, String path, boolean deleteSelf) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static async Task deleteChildren(ZooKeeper zookeeper, string path, bool deleteSelf)
        {
            PathUtils.validatePath(path);

            IList<string> children = (await zookeeper.getChildrenAsync(path, null)).Children;
            foreach (var child in children)
            {
                var fullPath = makePath(path, child);
                await deleteChildren(zookeeper, fullPath, true);
            }

            if (deleteSelf)
            {
                try
                {
                    await zookeeper.deleteAsync(path, -1);
                }
                catch (KeeperException.NotEmptyException)
                {
                    //someone has created a new child since we checked ... delete again.
                    await deleteChildren(zookeeper, path, true);
                }
                catch (KeeperException.NoNodeException)
                {
                    // ignore... someone else has deleted the node it since we checked
                }
            }
        }

        /// <summary>
        ///     Return the children of the given path sorted by sequence number
        /// </summary>
        /// <param name="zookeeper"> the client </param>
        /// <param name="path">      the path </param>
        /// <returns> sorted list of children </returns>
        /// <exception cref="InterruptedException">                 thread interruption </exception>
        /// <exception cref="org.apache.zookeeper.KeeperException"> zookeeper errors </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.util.List<String> getSortedChildren(org.apache.zookeeper.ZooKeeper zookeeper, String path) throws InterruptedException, org.apache.zookeeper.KeeperException
        public static async Task<IList<string>> getSortedChildren(ZooKeeper zookeeper, string path)
        {
            IList<string> children = (await zookeeper.getChildrenAsync(path, false)).Children;
            var sortedList = new List<string>(children);
            sortedList.Sort();
            return sortedList;
        }

        /// <summary>
        ///     Given a parent path and a child node, create a combined full path
        /// </summary>
        /// <param name="parent"> the parent </param>
        /// <param name="child">  the child </param>
        /// <returns> full path </returns>
        public static string makePath(string parent, string child)
        {
            var path = new StringBuilder();

            joinPath(path, parent, child);

            return path.ToString();
        }

        /// <summary>
        ///     Given a parent path and a list of children nodes, create a combined full path
        /// </summary>
        /// <param name="parent">       the parent </param>
        /// <param name="firstChild">   the first children in the path </param>
        /// <param name="restChildren"> the rest of the children in the path </param>
        /// <returns> full path </returns>
        public static string makePath(string parent, string firstChild, params string[] restChildren)
        {
            var path = new StringBuilder();

            joinPath(path, parent, firstChild);

            if (restChildren == null)
            {
                return path.ToString();
            }
            foreach (var child in restChildren)
            {
                joinPath(path, "", child);
            }

            return path.ToString();
        }

        /// <summary>
        ///     Given a parent and a child node, join them in the given <seealso cref="StringBuilder path" />
        /// </summary>
        /// <param name="path">   the <seealso cref="StringBuilder" /> used to make the path </param>
        /// <param name="parent"> the parent </param>
        /// <param name="child">  the child </param>
        private static void joinPath(StringBuilder path, string parent, string child)
        {
            // Add parent piece, with no trailing slash.
            if ((parent != null) && (parent.Length > 0))
            {
                if (!parent.StartsWith(PATH_SEPARATOR, StringComparison.Ordinal))
                {
                    path.Append(PATH_SEPARATOR);
                }
                if (parent.EndsWith(PATH_SEPARATOR, StringComparison.Ordinal))
                {
                    path.Append(parent.Substring(0, parent.Length - 1));
                }
                else
                {
                    path.Append(parent);
                }
            }

            if ((child == null) || (child.Length == 0) || child.Equals(PATH_SEPARATOR))
            {
                // Special case, empty parent and child
                if (path.Length == 0)
                {
                    path.Append(PATH_SEPARATOR);
                }
                return;
            }

            // Now add the separator between parent and child.
            path.Append(PATH_SEPARATOR);

            if (child.StartsWith(PATH_SEPARATOR, StringComparison.Ordinal))
            {
                child = child.Substring(1);
            }

            if (child.EndsWith(PATH_SEPARATOR, StringComparison.Ordinal))
            {
                child = child.Substring(0, child.Length - 1);
            }

            // Finally, add the child.
            path.Append(child);
        }

        private static CreateMode getCreateMode(bool asContainers)
        {
            return asContainers ? getContainerCreateMode() : CreateMode.PERSISTENT;
        }

        private class CreateModeHolder
        {
            private static readonly TraceLogger log = TraceLogger.GetLogger(typeof(ZKPaths));
            internal static readonly CreateMode containerCreateMode = NON_CONTAINER_MODE;
            
        }

        public class PathAndNode
        {
            internal readonly string node;
            internal readonly string path;

            public PathAndNode(string path, string node)
            {
                this.path = path;
                this.node = node;
            }

            public virtual string getPath()
            {
                return path;
            }

            public virtual string getNode()
            {
                return node;
            }
        }
    }
}