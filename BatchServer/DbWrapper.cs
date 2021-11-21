using System;

using DidasUtils;
using DidasUtils.Logging;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace BatchServer
{
    public class DbWrapper
    {
        MySqlConnection connection;


        [Obsolete("Man add a damn password system that does not need it hard coded (so it's not on github lol).")]
        public void Connect()
        {
            //load pass from file?
            string connectionStr = "server=localhost;user=batchServer;port=3306;password=server"; //FIXME: add a proper password??
            connection = new(connectionStr);
            
            try
            {
                Log.LogEvent("Connecting to MySql...", "DbWrapper");
                connection.Open();

                Log.LogEvent($"Connected to MySql. Server version: {connection.ServerVersion}", "DbWrapper");
            }
            catch (Exception e)
            {
                Log.LogException("Failed to connect to MySql.", "DbWrapper", e);
            }
        }
        public void Disconnect()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
        public void Dispose()
        {
            Disconnect();

            if (connection != null)
                connection.Dispose();

            connection = null;
        }



        public MySqlDataReader SendCommandDataReader(string sql)
        {
            MySqlCommand cmd = new(sql, connection);
            return cmd.ExecuteReader();
        }
        public int SendCommandNonQuery(string sql)
        {
            MySqlCommand cmd = new(sql, connection);
            return cmd.ExecuteNonQuery();
        }
        public object SendCommandScalar(string sql)
        {
            MySqlCommand cmd = new(sql, connection);
            return cmd.ExecuteScalar();
        }
    }
}
