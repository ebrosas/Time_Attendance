using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting.Expressions;

namespace GARMCO.AMS.TAS.ReportLib
{
    public static class ReportHelper
    {
        #region Public Methods
        public static string ConvertMinuteToHour(int minuteValue)
        {
            string result = string.Empty;

            try
            {
                int hrs = 0;
                int min = 0;

                hrs = Math.DivRem(Convert.ToInt32(minuteValue), 60, out min);
                result = string.Format("{0}:{1}",
                    string.Format("{0:00}", hrs),
                    string.Format("{0:00}", min));

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
