using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using TextLogs;

namespace mssql.dbman
{
    public class MSSQLServer
    {
        private SqlConnection conn = new SqlConnection();
        public int timeout { get; set; }
        private string connectionString;
        public string server { get; set; }
        public string database { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string output { get; set; }
        private Int64 identity_scope { get; set; }
        public bool debugMode { get; set; }
        private bool writeLogs { get; set; } = false;
        private Log log;
        public MSSQLServer(int timeout = 600, Log log = null)
        {
            this.timeout = timeout;

            if (log != null)
            {
                this.log = log;
                writeLogs = true;
            }
        }

        public bool SetConnection(string server = null, string database = null, string user = null, string password = null)
        {
            bool success = false;
            try
            {
                if (server == null || database == null || user == null || password == null)
                {
                    server = this.server;
                    database = this.database;
                    user = this.user;
                    password = this.password;
                }
                conn.ConnectionString = $"Server={server}; Database={database}; User ID={user}; Password={password}";
                conn.Open();
                success = true;

                if (writeLogs)
                    this.log.Write($"Successful connection established with sql client: {conn.ConnectionString}");
            } catch (SqlException ex)
            {
                success = false;

                if (writeLogs)
                    this.log.Write($"Sql client connection error: {ex.Message} | {ex.HResult}", true);
#if DEBUG
                Debug.Print($"Sql client connection error: {ex.Message} | {ex.HResult}", true);
#endif
            }
            finally
            {
                conn.Close();
            }
            return success;
        }

        public DataTable GetData(string query, bool newScope = true)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da;
            try
            {
                if (newScope)
                    this.conn.Open();
#if DEBUG
                Debug.Print($"Executing sql statement: {query}");
#endif
                if (debugMode && writeLogs)
                    this.log.Write($"Executing sql statement: {query}");

                da = new SqlDataAdapter(query, conn);
                da.SelectCommand.CommandTimeout = timeout;
                da.Fill(dt);
                da.Dispose();
                da = null;
                if (newScope)
                    conn.Close();
            } catch (SqlException ex)
            {
                if (writeLogs)
                    this.log.Write($"Sql exception on query execution: {ex.Message} | {ex.HResult} | Line: {ex.LineNumber}", true);
#if DEBUG
                Debug.Print($"Sql exception on query execution: {ex.Message} | {ex.HResult} | Line: {ex.LineNumber}");
#endif
            }
            finally
            {
                if (newScope)
                    conn.Close();
            }
            return dt;
        }

        public bool WriteData(string query)
        {
            bool success = false;
            try
            {
                SqlCommand command = new SqlCommand(query, this.conn);
                conn.Open();
                command.CommandTimeout = this.timeout;
                command.ExecuteScalar();
                success = true;
                if (debugMode && writeLogs)
                    this.log.Write($"Executing sql statement: {query}");
#if DEBUG
                Debug.Print($"Executing sql statement: {query}");
#endif
            } catch (SqlException ex)
            {
                if (writeLogs)
                    this.log.Write($"Sql exception on query execution: {ex.Message} | {ex.HResult} | {ex.LineNumber}");
#if DEBUG
                Debug.Print($"Sql exception on query execution: {ex.Message} | {ex.HResult} | Line: {ex.LineNumber}");
#endif
            }
            finally
            {
                conn.Close();
            }
            return success;
        }

        private bool CheckColumns(DataTable dt, string tableName, bool caseSensitive = true)
        {
            bool success = true;
            try
            {
                if (writeLogs)
                    log.Write($"Checking column names (case sensitive = {caseSensitive.ToString()})");
                foreach (DataColumn col in dt.Columns)
                {
                    if (Convert.ToInt32(GetData($"SELECT COUNT(1) FROM sys.columns WHERE Name=N'{col.ColumnName}' COLLATE Latin1_General_CS_AS AND Object_ID=Object_ID(N'{tableName}')").Rows[0][0].ToString()) == 1)
                    {
                        if (writeLogs)
                            log.Write($"  Column {col.ColumnName} - Matched");
#if DEBUG
                        Debug.Print($"  Column {col.ColumnName} - Matched");
#endif
                    }
                    else
                    {
                        if (writeLogs)
                            log.Write($"  Column '{col.ColumnName}'. Can't find matching column on destination. Verify.", true);
#if DEBUG
                        Debug.Print($"  Column '{col.ColumnName}'. Can't find matching column on destination. Verify.");
#endif
                        success = false;
                        break;
                    }
                }
            } catch (Exception ex)
            {
                success = false;
                if (writeLogs)
                    log.Write($"Error checking columns: {ex.Message} | {ex.HResult}", true);
#if DEBUG
                Debug.Print($"Error checking columns: {ex.Message} | {ex.HResult}");
#endif
            }
            return success;
        }

        private bool CheckDataTypes(DataTable dt, string destinationTable)
        {
            bool success = false;
            try
            {
                if (writeLogs)
                    log.Write($"Checking data types on column matches from table {destinationTable}");

#if DEBUG
                Debug.Print($"Checking data types on column matches from table {destinationTable}");
#endif
                DataTable dataTypes = GetData($"SELECT TOP 0 * FROM {destinationTable} WITH(NOLOCK)");
                foreach (DataColumn colA in dt.Columns)
                {
                    foreach (DataColumn colB in dataTypes.Columns)
                        if (colA.ColumnName == colB.ColumnName)
                            if (colA.DataType == colB.DataType)
                            {
                                success = true;
                                if (writeLogs)
                                    log.Write($"  - Column {colA.ColumnName}: OK");
                            }
                            else
                            {
                                success = false;
                                if (writeLogs)
                                    log.Write($"  - Column {colA.ColumnName}: ERROR. The data type ({colA.DataType}) on column '{colA.ColumnName}' doesn't match with column '{colB.ColumnName}' (datatype {colB.DataType})", true);
                                break;
                            }
                    if (!success)
                        break;
                }
            } catch (Exception ex)
            {
                if (writeLogs)
                    log.Write($"Error checking data types of table {destinationTable}: {ex.Message} | {ex.HResult}");
#if DEBUG
                Debug.Print($"Error checking data types of table {destinationTable}: {ex.Message} | {ex.HResult}");
#endif
            }
            return success;
        }

        public bool BulkInsert(DataTable dt, string destinationTable, bool columnNamesMatch = true, bool getIdentity = false)
        {
            bool success = false;
            try
            {
                if (CheckDataTypes(dt, destinationTable))
                    if (CheckColumns(dt, destinationTable))
                        if (SetConnection())
                        {
                            if (writeLogs)
                                log.Write($"Bulk insert started");
#if DEBUG
                            Debug.Print($"Bulk insert started");
#endif
                            this.conn.Open();
                            using (SqlBulkCopy bcopy = new SqlBulkCopy(conn))
                            {
                                bcopy.BulkCopyTimeout = this.timeout;
                                bcopy.DestinationTableName = destinationTable;

                                if (columnNamesMatch)
                                    foreach (DataColumn i in dt.Columns)
                                    {
                                        SqlBulkCopyColumnMapping colMapping = new SqlBulkCopyColumnMapping(i.ColumnName, i.ColumnName);
                                        bcopy.ColumnMappings.Add(colMapping);
                                    }
                                bcopy.WriteToServer(dt);
                                bcopy.Close();

                                if (getIdentity)
                                    identity_scope = Convert.ToInt64(GetData($"SELECT ISNULL(SCOPE_IDENTITY(),0) [SCOPE_IDENTITY]", newScope: false).Rows[0][0].ToString());

                                if (writeLogs)
                                    log.Write($"{dt.Rows.Count} lines copied to destination");
#if DEBUG
                                Debug.Print($"{dt.Rows.Count} lines copied to destination");
#endif
                            }
                            success = true;
                        }
            } catch (Exception ex)
            {
                if (writeLogs)
                    log.Write($"Error in bulk insert: {ex.Message} | {ex.HResult}", true);
#if DEBUG
                Debug.Print($"Error in bulk insert: {ex.Message} | {ex.HResult}");
#endif
            }
            return success;
        }

        private void Print(string message, bool isError = false)
        {

        }
    }
}
