using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquintScript.Helpers
{
    public static class Logger
    {
        private static string logpath = @"\\spvaimapcn\data$\Apps\Squint\Logs\Log.txt";
        public static void AddLog(string log_entry)
        {
            using (var data = new System.IO.StreamWriter(logpath, true))
            {
                data.WriteLine(log_entry);
            }
        }
    }
}
