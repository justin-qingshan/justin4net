using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace just4net.db
{
    public sealed class SqlDB : IDB, IDisposable
    {
        private const int TIMEOUT = 3;

        private string connStr;
        private SqlConnection conn;
        private SqlTransaction tran;

        private bool disposed = false;


        public SqlDB(string connStr)
        {
            this.connStr = connStr;
        }


        public void Open(bool transaction = false)
        {
            if (disposed)
                return;

            if (conn == null)
                conn = new SqlConnection(connStr);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to open connection: " + connStr, ex);
            }

            if (transaction)
                tran = conn.BeginTransaction();
        }


        public void Close()
        {
            if (disposed)
                return;

            if (conn == null)
                return;

            if (conn.State == ConnectionState.Open)
                conn.Close();
        }


        /// <summary>
        /// Begin the transaction.
        /// </summary>
        public IDbTransaction BeginTransaction()
        {
            if (conn == null || conn.State != ConnectionState.Open || tran != null)
                return null;

            tran = conn.BeginTransaction();
            return tran;
        }


        /// <summary>
        /// Commit the transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (conn == null || conn.State != ConnectionState.Open || tran == null)
                return;

            tran.Commit();
        }


        /// <summary>
        /// Rollback the transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            if (conn == null || conn.State != ConnectionState.Open || tran == null)
                return;

            tran.Rollback();
        }


        /// <summary>
        /// Use command and parameters to query result of data table.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParam"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public DataTable QueryCommand(string cmdStr, CommandType cmdType, ICollection<IDataParameter> parameters = null, IDataParameter returnParam = null, int timeout = TIMEOUT)
        {
            SqlCommand cmd = GenerateCommand(cmdStr, cmdType, parameters, returnParam, timeout);

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            try
            {
                sda.Fill(dt);
            }
            catch(Exception ex)
            {
                throw GenerateException(ex, cmdStr, parameters);
            }

            return dt;
        }


        /// <summary>
        /// Use parameters to run a command, and return the rows count affected.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParam"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public int RunCommand(string cmdStr, CommandType cmdType, ICollection<IDataParameter> parameters = null, IDataParameter returnParam = null, int timeout = TIMEOUT)
        {
            SqlCommand cmd = GenerateCommand(cmdStr, cmdType, parameters, returnParam, timeout);

            int result;
            try
            {
                result = cmd.ExecuteNonQuery();
            }
            catch(Exception ex)     
            {
                throw GenerateException(ex, cmdStr, parameters);
            }

            return result;
        }

        
        /// <summary>
        /// Generate a command using command string and parameters.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <param name="returnValue"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public SqlCommand GenerateCommand(string cmdStr, CommandType cmdType,
            ICollection<IDataParameter> parameters, IDataParameter returnValue, int timeout)
        {
            SqlCommand cmd = GenerateCommand(cmdStr, cmdType, timeout);

            SetParameter(cmd, parameters);

            if (returnValue != null)
                cmd.Parameters.Add(returnValue);

            return cmd;
        }


        /// <summary>
        /// generate a command using command string.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public SqlCommand GenerateCommand(string cmdStr, CommandType cmdType, int timeout)
        {
            if (disposed)
                return null;

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = cmdStr;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = timeout;

            if (tran != null)
                cmd.Transaction = tran;

            return cmd;
        }


        /// <summary>
        /// Set parameter for command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SqlCommand SetParameter(SqlCommand cmd, ICollection<IDataParameter> parameters)
        {
            if (parameters == null)
                return cmd;

            foreach (SqlParameter param in parameters)
            {
                if (param.Direction == ParameterDirection.Input && param.Value == null)
                    param.Value = DBNull.Value;
                cmd.Parameters.Add(param);
            }

            return cmd;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing)
                {
                    if (conn != null)
                        conn.Dispose();
                    conn = null;
                }
            }
        }


        private ApplicationException GenerateException(Exception ex, string cmdStr, 
            ICollection<IDataParameter> parameters)
        {
            string msg = "Sql execute error：" + ex.Message + "; cmdStr:" + cmdStr + "; parameters：";
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    msg += param.ParameterName + "=" + param.Value + ",";
                }
            }
            return new ApplicationException(msg, ex);
        }
    }
}
