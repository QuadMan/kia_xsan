using EGSE.Utilites;
using EGSE.Utilites.ADC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kia_xan
{
    public class XsanTm
    {
        //калибровочные данные канала напряжения
        private static CalibrationValues adcUCbV = new CalibrationValues(
            new CValue[11]{
                new CValue((float)27740,(float)16.1),
                new CValue((float)31220,(float)18.1),
                new CValue((float)34670,(float)20.1),
                new CValue((float)39880,(float)23.1),
                new CValue((float)45080,(float)26.1),
                new CValue((float)46820,(float)27.1),
                new CValue((float)48560,(float)28.1),
                new CValue((float)50310,(float)29.1),
                new CValue((float)52050,(float)30.1),
                new CValue((float)55510,(float)32.1),
                new CValue((float)62240,(float)36.1)

            }
        );

        // калибровочные данные канала тока
        private static CalibrationValues adcICbV = new CalibrationValues(
            new CValue[11]{
                new CValue((float)220,(float)18),
                new CValue((float)360,(float)40),
                new CValue((float)830,(float)100),
                new CValue((float)1200,(float)150),
                new CValue((float)3180,(float)400),
                new CValue((float)6350,(float)800),
                new CValue((float)8000,(float)1000),
                new CValue((float)10400,(float)1300),
                new CValue((float)12000,(float)1500),
                new CValue((float)14500,(float)1800),
                new CValue((float)16130,(float)2000)
            }
        );
        private bool _isPowerOn;

        /// <summary>
        /// Индекс канала измерения напряжения
        /// </summary>
        public const uint ADC_CH_U = 0;

        /// <summary>
        /// Индекс канала измерения тока
        /// </summary>
        public const uint ADC_CH_I = 1;

        /// <summary>
        /// Показания АЦП (напряжение 27В, напряжение XSAN, ток XSAN)
        /// </summary>
        public ADC Adc;

        /// <summary>
        /// подано питание на LINA-XSAN
        /// </summary>
        public bool IsPowerOn
        {
            get
            {
                return _isPowerOn;
            }
        }

        public int lastData1;
        public int lastData2;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public XsanTm()
        {
            Adc = new ADC();
            // Создаем три канала со своими калибровочными характеристиками, устредняем по 10 значениям
            Adc.AddChannel(ADC_CH_U, adcUCbV, 10);
            //Adc.AddChannel(ADC_CH_U, null, 10);
            Adc.AddChannel(ADC_CH_I, adcICbV, 10);
            //
            _isPowerOn = false;
        }

        /// <summary>
        /// Обрабатываем данные телеметрии
        /// </summary>
        /// <param name="buf">Буфер с данными</param>
        public void Update(byte[] buf)
        {
            Adc.AddData(ADC_CH_U, ((int)buf[0] << 8) | buf[1]);
            lastData1 = ((int)buf[0] << 8) | buf[1];
            //Adc.AddData(ADC_CH_U, ((int)buf[2] << 8) | buf[3]);
            Adc.AddData(ADC_CH_I, ((int)buf[4] << 8) | buf[5]);
            lastData2 = ((int)buf[4] << 8) | buf[5];

            _isPowerOn = (buf[6] & 1) == 1;
        }
    }
}
