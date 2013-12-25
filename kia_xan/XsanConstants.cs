using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kia_xan
{
    static public class XsanConst
    {
        public const string SW_CAPTION = "КИА XSAN";
        //public const string SW_VERSION = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public const string DEV_NAME = "БИ КИА XSAN";
        public const string XSANSerial = "KIA_LINA";

        /// <summary>
        /// Индекс свойств в Control Values
        /// </summary>
        public const int PROPERTY_XSAN_READY_IDX = 0;
        public const int PROPERTY_XSAN_BUSY_IDX = 1;
        public const int PROPERTY_XSAN_ME_IDX = 2;
        public const int PROPERTY_XSAN_CMD_CH_IDX = 3;
        public const int PROPERTY_XSAN_DAT_CH_IDX = 4;

        public const int PROPERTY_BUNI_ON_IDX = 5;
        public const int PROPERTY_BUNI_CMD_CH_IDX = 6;
        public const int PROPERTY_BUNI_DAT_CH_IDX = 7;
        public const int PROPERTY_BUNI_HZ_IDX = 8;
        public const int PROPERTY_BUNI_KBV_IDX = 9;

        public const int PROPERTY_POWER_IDX = 10;

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
        public const int POWER_CTRL_IDX = 2;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
