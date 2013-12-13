using System;

namespace kia_xan
{

    /// <summary>
    /// Класс логгеров (синглетон)
    /// </summary>
    public sealed class LogsClass
    {
        private static string[] LogsFiles = new string[5] { "main.log", "operator.log", "hsi.log", "errors.log", "usb.log" };
        public enum Idx { logMain, logOperator, logHSI, logErrors, logUSB };

        private static volatile LogsClass instance;
        private static object syncRoot = new Object();

        public TxtLoggers Files;

        private LogsClass()
        {
            Files = new TxtLoggers();
            foreach (string FName in LogsFiles)
            {
                Files.AddFile(FName);
            }
        }

        public static LogsClass Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new LogsClass();
                    }
                }

                return instance;
            }
        }
    }
}