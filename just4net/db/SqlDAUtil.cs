using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace just4net.db
{
    public class SqlDAUtil
    {
        private SqlDB db;
        private CommandType cmdType;
        private string cmdStr;

        private List<IDataParameter> parameters;

        /// <summary>
        /// Create a Command from sql text.
        /// </summary>
        /// <param name="db">IDB.</param>
        /// <param name="cmdStr">command string.</param>
        /// <returns></returns>
        public static SqlDAUtil FromText(SqlDB db, string cmdStr)
        {
            return new SqlDAUtil(db, CommandType.Text, cmdStr);
        }


        /// <summary>
        /// Create a Command from stored procedure.
        /// </summary>
        /// <param name="db">IDB.</param>
        /// <param name="cmdStr">sql procedure's name to call.</param>
        /// <returns></returns>
        public static SqlDAUtil FromProcedure(SqlDB db, string cmdStr)
        {
            return new SqlDAUtil(db, CommandType.StoredProcedure, cmdStr);
        }


        private SqlDAUtil(SqlDB db, CommandType cmdType, string cmdStr)
        {
            this.db = db;
            this.cmdType = cmdType;
            this.cmdStr = cmdStr;
            parameters = new List<IDataParameter>();
        }


        /// <summary>
        /// Add parameter to current Command.
        /// </summary>
        /// <param name="paramName">parameter's name</param>
        /// <param name="value">parameter's value</param>
        /// <returns></returns>
        public SqlDAUtil AddParam(string paramName, object value)
        {
            parameters.Add(new SqlParameter(paramName, value));
            return this;
        }


        /// <summary>
        /// Add parameter to current Command.
        /// </summary>
        /// <param name="paramName">parameter's name</param>
        /// <param name="value">paremter's value</param>
        /// <param name="dbType">type in sql server.</param>
        /// <returns></returns>
        public SqlDAUtil AddParam(string paramName, object value, SqlDbType dbType)
        {
            SqlParameter p = new SqlParameter(paramName, dbType);
            p.Value = value;
            parameters.Add(p);
            return this;
        }


        /// <summary>
        /// Add parameter to current Command.
        /// </summary>
        /// <param name="paramName">parameter's name</param>
        /// <param name="value">parameter's value</param>
        /// <param name="dbType">data type in sql server.</param>
        /// <param name="size">size of value</param>
        /// <returns></returns>
        public SqlDAUtil AddParam(string paramName, object value, SqlDbType dbType, int size)
        {
            SqlParameter param = new SqlParameter(paramName, dbType, size);
            param.Value = value;
            parameters.Add(param);
            return this;
        }


        /// <summary>
        /// Use this command to query and return data table.
        /// </summary>
        /// <returns></returns>
        public DataTable Query()
        {
            return db.QueryCommand(cmdStr, cmdType, parameters);
        }

        /// <summary>
        /// Use this command to run sql text and return the rows count affected.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            return db.RunCommand(cmdStr, cmdType, parameters);
        }


        /// <summary>
        /// Use this command to query. Return data table with an out parameter's value.
        /// </summary>
        /// <param name="returnValue">-1 if return parameter's value is null.</param>
        /// <returns></returns>
        public DataTable Query(out int returnValue)
        {
            IDataParameter returnParam = new SqlParameter("@RETURN", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;
            DataTable dt = db.QueryCommand(cmdStr, cmdType, parameters, returnParam);
            returnValue = returnParam.Value == null ? -1 : Convert.ToInt32(returnParam.Value);
            return dt;
        }


        /// <summary>
        /// Use this command to run sql text. Return data table with an out parameter's value.
        /// </summary>
        /// <param name="returnValue">-1 if return parameter's value is null.</param>
        /// <returns></returns>
        public int Run(out int returnValue)
        {
            IDataParameter returnParam = new SqlParameter("@RETURN", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;
            int count = db.RunCommand(cmdStr, cmdType, parameters, returnParam);
            returnValue = returnParam.Value == null ? -1 : Convert.ToInt32(returnParam.Value);
            return count;
        }
        
    }

    public static class SqlDBUtil
    {
        public static SqlDAUtil FromProcedure(this SqlDB db, string procedure)
        {
            return SqlDAUtil.FromProcedure(db, procedure);
        }

        public static SqlDAUtil FromText(this SqlDB db, string cmdStr)
        {
            return SqlDAUtil.FromText(db, cmdStr);
        }
    }
}
