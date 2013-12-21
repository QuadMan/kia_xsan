using EGSE.Utilites;
using System;

namespace kia_xan
{
    /// <summary>
    /// Класс логгеров (синглетон)
    /// </summary>
    public sealed class LogsClass //: IDisposable
    {
        private static string[] LogsFiles = new string[5] 
            { 
                "main.log", 
                "operator.log", 
                "hsi.log", 
                "errors.log", 
                "usb.log" 
            };
        public const int MainIdx = 0;
        public const int OperatorIdx = 1;
        public const int HsiIdx = 2;
        public const int ErrorsIdx = 3;
        public const int UsbIdx = 4;

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

        //public void Dispose()
        //{
        //    Files[0].Dispose();
        //}
    }
}