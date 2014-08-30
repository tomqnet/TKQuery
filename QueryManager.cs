using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKQuery
{
    /// <summary>
    /// Class used to 
    /// </summary>
    public class QueryManager
    {
        private IDbConnection connection;
        private long queriesCount;        
        private IDbTransaction transaction;
        private int MAX_RETRY_COUNT = 3;

        public IDbTransaction Transaction
        {
            get { return transaction; }
        }

        public long QueriesCount { get { return queriesCount; } }
                
        public QueryManager(IDbConnection connection)
        {
            this.connection = connection;
        }

        private IDictionary<string, object> GetData(IDataReader dataReader)
        {
            var data = new Dictionary<string, object>();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                var name = dataReader.GetName(i);
                var value = dataReader.GetValue(i);
                data.Add(name, value);
            }
            return data;
        }

        private List<IDictionary<string, object>> ExecuteQuery(Query query, int retryCount = 0)
        {
            try
            {

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                List<IDictionary<string, object>> datas = new List<IDictionary<string, object>>();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = query.Text;
                command.Transaction = transaction;
                foreach (var parameter in query.Parameters)
                {
                    IDbDataParameter dbDataParameter = command.CreateParameter();
                    dbDataParameter.ParameterName = parameter.Name;
                    dbDataParameter.Value = parameter.Value == null ? DBNull.Value : parameter.Value;
                    command.Parameters.Add(dbDataParameter);
                }
                IDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var data = GetData(dataReader);
                    datas.Add(data);
                }
                dataReader.Close();
                queriesCount++;

                if (connection.State != ConnectionState.Closed && transaction == null)
                {
                    connection.Close();
                }

                return datas;
            }
            catch (Exception ex)
            {
                if (retryCount == MAX_RETRY_COUNT) 
                {
                    throw ex;
                }
                System.Threading.Thread.Sleep(250 + DateTime.Now.Millisecond);
                return ExecuteQuery(query, ++retryCount);
            }
        }

        /// <summary>
        /// Executes single query
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="query"></param>
        public void Execute(Query query)
        {
            ExecuteQuery(query);
        }

        /// <summary>
        /// Opens connections and set transaction
        /// </summary>
        public void BeginTransaction()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (transaction == null)
            {
                transaction = connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Opens connections and set transaction
        /// </summary>
        public void BeginTransaction(IsolationLevel il)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (transaction == null)
            {
                transaction = connection.BeginTransaction(il);
            }
        }

        /// <summary>
        /// Commits transaction and closes connection
        /// </summary>
        public void Commit()
        {
            if (transaction == null)
            {
                throw new NullReferenceException("Transaction was not started");
            }
            transaction.Commit();
            transaction = null;

            CloseConnection();
        }

        /// <summary>
        /// Rollbacks transaction and closes connection
        /// </summary>
        public void Rollback()
        {
            if (transaction == null)
            {
                throw new NullReferenceException("Transaction was not started");
            }
            transaction.Rollback();
            transaction = null;

            CloseConnection();
        }

        private void CloseConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public T1 Execute<T1>(Query query)
        {
            var datas = ExecuteQuery(query);
            Mapper<T1> mapper = new Mapper<T1>();
            var result = mapper.Map(datas);
            return result;
        }
    }
}
