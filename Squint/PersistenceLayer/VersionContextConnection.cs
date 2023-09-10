using System.Data.Common;

namespace Squint
{
    public static class VersionContextConnection
    {
        //private static string providerName = "Npgsql";
        private static string providerName = "System.Data.SqlClient";
        public static string databaseName = ""; // populated from Config.XML file
        private static string userName = "postgres";
        private static string password = "bccacn"; // not saved in XML for superficial security, but NOT not secure from inspection of executable by MSIL tools
        private static string host = @"CACNPC00030";
        private static int port = 1433;

        public static DbConnection GetDatabaseConnection()
        {
            var conn = DbProviderFactories.GetFactory(providerName).CreateConnection();
            conn.ConnectionString = $"Server={host}; "  
                + $"Initial Catalog={databaseName};"
                + $"Integrated Security=SSPI;";
            return conn;
        }
        public static string ConnectionString()
        {
            return $"Data Source={host}; " + $"Initial Catalog={databaseName};" + $"Integrated Security=SSPI;";
        }
    }
}
