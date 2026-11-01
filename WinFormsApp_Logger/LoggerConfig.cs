using System;

namespace WinFormsApp_Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class LoggerConfig
    {
        public bool AppendToFile { get; set; } = true;
        public int QueueNumber { get; set; } = 3;
        public string FileName { get; set; } = "application_log.txt";
    }
}