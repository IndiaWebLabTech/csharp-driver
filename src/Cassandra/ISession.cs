﻿//
//      Copyright (C) DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Cassandra
{
    /// <summary>
    /// A session holds connections to a Cassandra cluster, allowing it to be queried.
    /// <para>
    /// Each session maintains multiple connections to the cluster nodes,
    /// provides policies to choose which node to use for each query (round-robin on
    /// all nodes of the cluster by default), and handles retries for failed query (when
    /// it makes sense), etc...
    /// </para>
    /// <para>
    /// Session instances are thread-safe and usually a single instance is enough
    /// per application. However, a given session can only be set to one keyspace
    /// at a time, so one instance per keyspace is necessary.
    /// </para>
    /// </summary>
    public interface ISession: IDisposable
    {
        /// <summary>
        /// Gets the Cassandra native binary protocol version
        /// </summary>
        int BinaryProtocolVersion { get; }

        /// <summary>
        /// Gets the cluster information and state
        /// </summary>
        ICluster Cluster { get; }

        /// <summary>
        /// Determines if the object has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets name of currently used keyspace. 
        /// </summary>
        string Keyspace { get; }

        /// <summary>
        /// Gets the user defined type mappings
        /// </summary>
        UdtMappingDefinitions UserDefinedTypes { get; }

        /// <summary>
        /// Begins asynchronous execute operation.
        /// </summary>
        IAsyncResult BeginExecute(IStatement statement, AsyncCallback callback, object state);

        /// <summary>
        /// Begins asynchronous execute operation
        /// </summary>
        IAsyncResult BeginExecute(string cqlQuery, ConsistencyLevel consistency, AsyncCallback callback, object state);

        /// <summary>
        /// Begins asynchronous prepare operation
        /// </summary>
        IAsyncResult BeginPrepare(string cqlQuery, AsyncCallback callback, object state);

        /// <summary>
        /// Switches to the specified keyspace.
        /// </summary>
        /// <param name="keyspaceName">Case-sensitive name of keyspace to be used.</param>
        /// <exception cref="InvalidQueryException">When keyspace does not exist</exception>
        void ChangeKeyspace(string keyspaceName);
        string CurrantKeySpace { get; }
        /// <summary>
        ///  Creates new keyspace in current cluster.        
        /// </summary>
        /// <param name="keyspaceName">Case-sensitive name of keyspace to be created.</param>
        /// <param name="replication">
        /// Replication property for this keyspace.
        /// To set it, refer to the <see cref="ReplicationStrategies"/> class methods. 
        /// It is a dictionary of replication property sub-options where key is a sub-option name and value is a value for that sub-option. 
        /// <para>Default value is <c>SimpleStrategy</c> with <c>replication_factor = 1</c></para>
        /// </param>
        /// <param name="durableWrites">Whether to use the commit log for updates on this keyspace. Default is set to <c>true</c>.</param>
        void CreateKeyspace(string keyspaceName, Dictionary<string, string> replication = null, bool durableWrites = true);

        /// <summary>
        ///  Creates new keyspace in current cluster.
        ///  If keyspace with specified name already exists, then this method does nothing.
        /// </summary>
        /// <param name="keyspaceName">Case-sensitive name of keyspace to be created.</param>
        /// <param name="replication">
        /// Replication property for this keyspace.
        /// To set it, refer to the <see cref="ReplicationStrategies"/> class methods. 
        /// It is a dictionary of replication property sub-options where key is a sub-option name and value is a value for that sub-option.
        /// <para>Default value is <c>'SimpleStrategy'</c> with <c>'replication_factor' = 2</c></para>
        /// </param>
        /// <param name="durableWrites">Whether to use the commit log for updates on this keyspace. Default is set to <c>true</c>.</param>
        void CreateKeyspaceIfNotExists(string keyspaceName, Dictionary<string, string> replication = null, bool durableWrites = true);

        /// <summary>
        ///  Deletes specified keyspace from current cluster.
        ///  If keyspace with specified name does not exist, then exception will be thrown.
        /// </summary>
        /// <param name="keyspaceName">Name of keyspace to be deleted.</param>
        void DeleteKeyspace(string keyspaceName);

        /// <summary>
        ///  Deletes specified keyspace from current cluster.
        ///  If keyspace with specified name does not exist, then this method does nothing.
        /// </summary>
        /// <param name="keyspaceName">Name of keyspace to be deleted.</param>
        void DeleteKeyspaceIfExists(string keyspaceName);

        /// <summary>
        /// Ends asynchronous execute operation
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        RowSet EndExecute(IAsyncResult ar);

        /// <summary>
        /// Ends asynchronous prepare operation
        /// </summary>
        PreparedStatement EndPrepare(IAsyncResult ar);
        
        /// <summary>
        /// Executes the provided statement with the provided execution profile.
        /// The execution profile must have been added previously to the Cluster using <see cref="Builder.WithExecutionProfiles"/>.
        /// </summary>
        /// <param name="statement">Statement to execute.</param>
        /// <param name="executionProfileName">ExecutionProfile name to be used while executing the statement.</param>
        RowSet Execute(IStatement statement, string executionProfileName);

        /// <summary>
        /// Executes the provided query.
        /// </summary>
        RowSet Execute(IStatement statement);

        /// <summary>
        /// Executes the provided query.
        /// </summary>
        RowSet Execute(string cqlQuery);
        
        /// <summary>
        /// Executes the provided query with the provided execution profile.
        /// The execution profile must have been added previously to the Cluster using <see cref="Builder.WithExecutionProfiles"/>.
        /// </summary>
        /// <param name="cqlQuery">Query to execute.</param>
        /// <param name="executionProfileName">ExecutionProfile name to be used while executing the statement.</param>
        RowSet Execute(string cqlQuery, string executionProfileName);
        
        /// <summary>
        /// Executes the provided query.
        /// </summary>
        RowSet Execute(string cqlQuery, ConsistencyLevel consistency);

        /// <summary>
        /// Executes the provided query.
        /// </summary>
        RowSet Execute(string cqlQuery, int pageSize);

        /// <summary>
        /// Executes a query asynchronously
        /// </summary>
        /// <param name="statement">The statement to execute (simple, bound or batch statement)</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<RowSet> ExecuteAsync(IStatement statement);

        /// <summary>
        /// Executes a query asynchronously with the provided execution profile.
        /// The execution profile must have been added previously to the Cluster using <see cref="Builder.WithExecutionProfiles"/>.
        /// </summary>
        /// <param name="statement">The statement to execute (simple, bound or batch statement)</param>
        /// <param name="executionProfileName">ExecutionProfile name to be used while executing the statement.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<RowSet> ExecuteAsync(IStatement statement, string executionProfileName);

        /// <summary>
        /// Prepares the provided query string.
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        PreparedStatement Prepare(string cqlQuery);
        
        /// <summary>
        /// Prepares the query string, sending the custom payload request.
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <param name="customPayload">Custom outgoing payload to send with the prepare request</param>
        PreparedStatement Prepare(string cqlQuery, IDictionary<string, byte[]> customPayload);
        
        /// <summary>
        /// Prepares the provided query string asynchronously.
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        Task<PreparedStatement> PrepareAsync(string cqlQuery);
        
        /// <summary>
        /// Prepares the provided query string asynchronously, and sending the custom payload request.
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <param name="customPayload">Custom outgoing payload to send with the prepare request</param>
        Task<PreparedStatement> PrepareAsync(string cqlQuery, IDictionary<string, byte[]> customPayload);

        /// <summary>
        /// Prepares the provided query string asynchronously.
        /// Prepare or Get Prepared Statement from Concusrrent Disctionary Usefull when dealling multiple kespace on single Cluster Session.
        /// for using dynamic keyspace inject feature prefix @db. to table name e.g. <example>SELECT * FROM @db.mytable where mycolumn=?</example>
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <returns></returns>
        Task<PreparedStatement> PrepareOrGetAsync(string cqlQuery);
        /// <summary>
        /// Prepares the provided query string asynchronously.
        /// Prepare or Get Prepared Statement from Concusrrent Disctionary Usefull when dealling multiple kespace on single Cluster Session.
        /// for using dynamic keyspace inject feature prefix @db. to table name e.g. <example>SELECT * FROM @db.mytable where mycolumn=?</example>
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <param name="keyspace">keyspace name</param>
        /// <returns></returns>
        Task<PreparedStatement> PrepareOrGetAsync(string cqlQuery, string keyspace);
        /// <summary>
        /// Prepares the provided query string asynchronously, and sending the custom payload request.
        /// Prepare or Get Prepared Statement from Concusrrent Disctionary Usefull when dealling multiple kespace on single Cluster Session.
        /// for using dynamic keyspace inject feature prefix @db. to table name e.g. <example>SELECT * FROM @db.mytable where mycolumn=?</example>
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <param name="customPayload">Custom outgoing payload to send with the prepare request</param>
        /// <param name="keyspace">keyspace name</param>
        Task<PreparedStatement> PrepareOrGetAsync(string cqlQuery, IDictionary<string, byte[]> customPayload, string keyspace);
        /// <summary>
        /// Prepares the provided query string asynchronously, and sending the custom payload request.
        /// Prepare or Get Prepared Statement from Concusrrent Disctionary Usefull when dealling multiple kespace on single Cluster Session.
        /// for using dynamic keyspace inject feature prefix @db. to table name e.g. <example>SELECT * FROM @db.mytable where mycolumn=?</example>
        /// </summary>
        /// <param name="cqlQuery">cql query to prepare</param>
        /// <param name="customPayload">Custom outgoing payload to send with the prepare request</param>
        Task<PreparedStatement> PrepareOrGetAsync(string cqlQuery, IDictionary<string, byte[]> customPayload);
        [Obsolete("Method deprecated. The driver internally waits for schema agreement when there is an schema change. See ProtocolOptions.MaxSchemaAgreementWaitSeconds for more info.")]
        void WaitForSchemaAgreement(RowSet rs);
        [Obsolete("Method deprecated. The driver internally waits for schema agreement when there is an schema change. See ProtocolOptions.MaxSchemaAgreementWaitSeconds for more info.")]
        bool WaitForSchemaAgreement(IPEndPoint forHost);
    }
}
