using System.Collections.Generic;
using System.Data;

namespace just4net.db
{
    public interface IDB
    {

        /// <summary>
        /// Open connection.
        /// </summary>
        /// <param name="transaction"></param>
        void Open(bool transaction = false);


        /// <summary>
        /// Close connection.
        /// </summary>
        void Close();


        /// <summary>
        /// Begin transaction.
        /// </summary>
        IDbTransaction BeginTransaction();


        /// <summary>
        /// Commit transaction.
        /// </summary>
        void CommitTransaction();


        /// <summary>
        /// Rollback transaction.
        /// </summary>
        void RollbackTransaction();


        /// <summary>
        /// Execute query sql string, then return a data table.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParam"></param>
        /// <returns></returns>
        DataTable QueryCommand(string cmdStr, CommandType cmdType,
            ICollection<IDataParameter> parameters, IDataParameter returnParam, int timeout);


        /// <summary>
        /// Execute a executable sql string, then return rows' count affected.
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="cmdType"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParam"></param>
        /// <returns></returns>
        int RunCommand(string cmdStr, CommandType cmdType,
            ICollection<IDataParameter> parameters, IDataParameter returnParam, int timeout);
    }
}