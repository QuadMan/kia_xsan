using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kia_xan
{
    static public class XsanConst
    {
        public const string SW_CAPTION = "КИА XSAN";
        public const string SW_VERSION = "0.0.1.0";
        public const string DEV_NAME = "БИ КИА XSAN";
        public const string XSANSerial = "KIA_LINA";

        /// <summary>
        /// Индекс объекта управления XSAN в массиве ControlValuesList
        /// </summary>
        public const int XSAN_CTRL_IDX = 0;
        /// <summary>
        /// Индекс объекта управления BUNI в массиве ControlValuesList
        /// </summary>
        public const int BUNI_CTRL_IDX = 1;
        /// <summary>
        /// Индекс объекта управления POWER в массиве ControlValuesList
        /// </summary>
        public const int CTRL_POWER_IDX = 2;

        static public List<string> GetBUNICommandList()
        {
            return new List<string>
                {
                    "Команды по основному каналу",
                    "Команды по резервному каналу"
                };
        }

        static public List<string> GetBUNIDataList()
        {
            return new List<string>
                {
                    "Данные отключены",
                    "Данные по основному каналу",
                    "Данные по резервному каналу",
                    "Данные по всем каналам"
                };
        }

        static public List<string> GetXSANCmdList()
        {
            return new List<string>
                {
                    "Команды отключены",
                    "Команды по основному каналу",
                    "Команды по резервному каналу",
                    "Команды по всем каналам"
                };
        }

        static public List<string> GetXSANDataList()
        {
            return new List<string>
                {
                    "Данные отключены",
                    "Данные по основному каналу",
                    "Данные по резервному каналу",
                    "Данные по всем каналам"
                };
        }
    }
}
