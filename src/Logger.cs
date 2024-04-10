using System;
using System.IO;

namespace EasyScript
{
    public static class Logger
    {
        public static string LogFile = "Scripts/EasyScript/EasyScriptLog.txt";

        public static bool IsLogging = true;

        public static void Log(string message)
        {
            if (!IsLogging) return;
            File.AppendAllText(LogFile, $"{DateTime.Now} - {message}\n");
        }

        public static void Clear()
        {
            File.WriteAllText(LogFile, string.Empty);
        }
    }
}
