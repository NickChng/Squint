using System.Data.Common;

namespace SquintScript
{
    public static class VersionContextConnection
    {
        private static string providerName = "Npgsql";
        public static string databaseName = ""; // populated from Config.XML file
        private static string userName = "postgres";
        private static string password = "bccacn"; // not saved in XML for superficial security, but NOT not secure from inspection of executable by MSIL tools
        private static string host = "sprtqacn001";
        private static int port = 5432;

        public static DbConnection GetDatabaseConnection()
        {
            var conn = DbProviderFactories.GetFactory(providerName).CreateConnection();
            conn.ConnectionString = $"Server={host}; " + $"Port={port}; " +
                $"User Id={userName};" + $"Password={password};" + $"Database={databaseName};";
            return conn;
        }
        public static string ConnectionString()
        {
            return $"Server={host}; " + $"Port={port}; " + $"User Id={userName};" + $"Password={password};" + $"Database={databaseName};";
        }
    }
}
