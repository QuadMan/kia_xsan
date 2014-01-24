using System.Linq.Expressions;
using EGSE.Cyclogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EGSE.Utilites;

namespace kia_xan
{
    class XsanCyclogramCommands
    {
        public CyclogramCommands CycCommandsAvailable;
        //public List<ControlValue> ContolValuesList;
        public XSAN Xsan;
        public HSIWindow HsiWin;

        private List<string> XSAN_CMD_LIST = new List<string>()
        {
            "CMD_CH_NONE",
            "CMD_CH_MAIN",
            "CMD_CH_RES",
            "CMD_CH_MAIN_RES"
        };

        private List<string> XSAN_DAT_LIST = new List<string>()
        {
            "DAT_CH_NONE",
            "DAT_CH_MAIN",
            "DAT_CH_RES",
            "DAT_CH_MAIN_RES"
        };

        private List<string> BUNI_CMD_LIST = new List<string>()
        {
            "CMD_CH_MAIN",
            "CMD_CH_RES",
        };


        public XsanCyclogramCommands()
        {
            CycCommandsAvailable = new CyclogramCommands();
            CycCommandsAvailable.AddCommand("IMIT_XSAN", new CyclogramLine("IMIT_XSAN", XsanControlTest, XsanControlExec, String.Empty));
            CycCommandsAvailable.AddCommand("IMIT_BUNI", new CyclogramLine("IMIT_BUNI", BuniControlTest, BuniControlExec, String.Empty));
            CycCommandsAvailable.AddCommand("UKS", new CyclogramLine("UKS", UksTest, UksExec, String.Empty));
            CycCommandsAvailable.AddCommand("POWER", new CyclogramLine("POWER", PowerTest, PowerExec, String.Empty));
            CycCommandsAvailable.AddCommand("WRITE_XSAN_DATA", new CyclogramLine("WRITE_XSAN_DATA", WriteXsanDataTest, WriteXsanDataExec, String.Empty));
        }

        /// <summary>
        /// Команда XSAN 
        /// ON|OFF 
        /// CMD_CH_NONE|CMD_CH_MAIN|CMD_CH_RES|CMD_CH_MAIN_RES 
        /// DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES 
        /// BUSY_ON|BUSY_OFF
        /// ME_ON|ME_OFF
        /// В случае использования параметра XSAN ON необходимо прописать все значения
        /// В случае XSAN OFF ничего писать не нужно
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool XsanControlTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            switch (Params.Length)
            {
                case 5 :
                    if ((Params[0] != "ON") && (Params[0] != "OFF"))
                    {
                        errString = "Ошибка параметра: должно быть ON или OFF";
                        return false;                        
                    }

                    int idx = XSAN_CMD_LIST.IndexOf(Params[1]);
                    if (idx == -1)
                    {
                        errString = "Ошибка параметра: должно быть " + string.Join(" ", XSAN_CMD_LIST.ToArray());
                        return false;
                    }

                    idx = XSAN_DAT_LIST.IndexOf(Params[2]);
                    if (idx == -1)
                    {
                        errString = "Ошибка параметра: должно быть " + string.Join(" ", XSAN_DAT_LIST.ToArray());
                        return false;
                    }

                    if ((Params[3] != "BUSY_ON") && (Params[3] != "BUSY_OFF"))
                    {
                        errString = "Ошибка параметра: должно быть BUSY_ON или BUSY_OFF";
                        return false;                        
                    }

                    if ((Params[4] != "ME_ON") && (Params[4] != "ME_OFF"))
                    {
                        errString = "Ошибка параметра: должно быть ME_ON или ME_OFF";
                        return false;
                    }

                    break;
                default :
                    errString = "Ошибочное количество параметров команды!";
                    return false;
            }

            return true;
        }

        public bool XsanControlExec(string[] Params)
        {
            switch (Params.Length)
            {
                case 5:
                    int onOffParam = Params[0] == "ON" ? 1 : 0;
                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_READY_IDX, onOffParam, false);
                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_CMD_CH_IDX, XSAN_CMD_LIST.IndexOf(Params[1]), false);
                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_DAT_CH_IDX, XSAN_DAT_LIST.IndexOf(Params[2]), false);
                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_BUSY_IDX, Convert.ToInt16(Params[3] == "BUSY_ON"), false);
                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_ME_IDX, Convert.ToInt16(Params[4] == "ME_ON"));

                    Xsan.ControlValuesList[XsanConst.XSAN_CTRL_IDX].RefreshGetValue();  // вызываем для обновления галочек на экране
                    break;
                default:
                    return false;
            }
            return true;
        }

        const string BUNI_CMD_FORMAT_STR = "BUNI ON|OFF CMD_CH_MAIN|CMD_CH_RES DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES TIME_ON|TIME_OFF OBT_ON|OBT_OFF";
        /// <summary>
        /// Команда BUNI
        /// ON|OFF 
        /// CMD_CH_MAIN|CMD_CH_RES
        /// DAT_CH_NONE|DAT_CH_MAIN|DAT_CH_RES|DAT_CH_MAIN_RES 
        /// TIME_ON|TIME_OFF
        /// OBT_ON|OBT_OFF
        /// В случае использования параметра BUNI ON необходимо прописать все значения
        /// В случае BUNI OFF ничего писать не нужно
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool BuniControlTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            switch (Params.Length)
            {
                case 5:
                    if ((Params[0] != "ON") && (Params[0] != "OFF"))
                    {
                        errString = "Первый параметр должен быть ON или OFF";
                        return false;
                    }

                    int idx = BUNI_CMD_LIST.IndexOf(Params[1]);
                    if (idx == -1)
                    {
                        errString = "Второй параметр должен быть " + string.Join(" ", XSAN_CMD_LIST.ToArray());  //XSAN_CMD_LIST.ToString();
                        return false;
                    }

                    idx = XSAN_DAT_LIST.IndexOf(Params[2]);
                    if (idx == -1)
                    {
                        errString = "Третий параметр должен быть " + string.Join(" ", XSAN_DAT_LIST.ToArray());
                        return false;
                    }

                    if ((Params[3] != "TIME_ON") && (Params[3] != "TIME_OFF"))
                    {
                        errString = "Четвертый параметр должен быть TIME_ON | TIME_OFF";
                        return false;
                    }

                    if ((Params[4] != "OBT_ON") && (Params[4] != "OBT_OFF"))
                    {
                        errString = "Пятый параметр должен быть OBT_ON|OBT_OFF";
                        return false;
                    }

                    break;
                default:
                    errString = "Ошибочное количество параметров команды: "+BUNI_CMD_FORMAT_STR;
                    return false;
            }

            return true;
        }

        public bool BuniControlExec(string[] Params)
        {
            switch (Params.Length)
            {
                case 5:
                    int onOffParam = Params[0] == "ON" ? 1 : 0;
                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_ON_IDX, onOffParam, false);
                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_CMD_CH_IDX, BUNI_CMD_LIST.IndexOf(Params[1]), false);
                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_DAT_CH_IDX, XSAN_DAT_LIST.IndexOf(Params[2]), false);
                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_HZ_IDX, Convert.ToInt16(Params[3] == "TIME_ON"), false);
                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_KBV_IDX, Convert.ToInt16(Params[4] == "OBT_ON"));

                    Xsan.ControlValuesList[XsanConst.BUNI_CTRL_IDX].RefreshGetValue();
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Команда UKS - Выдача УКС в XSAN
        /// UKS BYTE1..BYTE62
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool UksTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            if ((Params == null) || (Params.Length > 62) || (Params.Length < 1))
            {
                errString = "Должно быть задано от 1 до 62 байт данных УКС";
                return false;
            }
            try
            {
                EGSE.Utilites.Converter.HexStrToByteArray(Params);
            }
            catch
            {
                errString ="Ошибка преобразования значений УКС";
                return false;
            }

            return true;
        }

        public bool UksExec(string[] Params)
        {
            byte[] UKSData = EGSE.Utilites.Converter.HexStrToByteArray(Params);
            Xsan.Device.CmdSendUKS(UKSData);

            return true;
        }
        

        /// <summary>
        /// Команда POWER - выдача питания XSAN
        /// POWER ON|OFF
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool PowerTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            if (!((Params.Length == 1) && ((Params[0] == "ON") || (Params[0] == "OFF"))))
            {
                errString = "Ошибка формата команды: должно быть указано ON или OFF";
                return false;
            }

            return true;
        }

        public bool PowerExec(string[] Params)
        {
            int val = Convert.ToInt32(Params[0] == "ON");
            return Xsan.ControlValuesList[XsanConst.POWER_CTRL_IDX].SetProperty(XsanConst.PROPERTY_POWER_IDX, val);
        }

        /// <summary>
        /// Команда WRITE_XSAN_DATA записываем данных с XSAN
        /// WRITE_XSAN_DATA ON|OFF
        /// </summary>
        /// <param name="Params"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public bool WriteXsanDataTest(string[] Params, out string errString)
        {
            errString = String.Empty;
            if (!((Params.Length == 1) && ((Params[0] == "ON") || (Params[0] == "OFF"))))
            {
                errString = "Ошибка формата команды: должно быть указано ON или OFF";
            }

            return true;
        }

        public bool WriteXsanDataExec(string[] Params)
        {
            Xsan.WriteXsanDataToFile = (Params[0] == "ON");

            return true;
        }
    }
}
