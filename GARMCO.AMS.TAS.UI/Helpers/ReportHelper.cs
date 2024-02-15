using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Helpers
{
    public static class ReportHelper
    {
        #region Public Methods
        public static string ConvertMinuteToHour(Int64 minuteValue)
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

        public static string ConvertMinuteToHourString(dynamic minuteValue)
        {
            string result = string.Empty;

            try
            {
                if (minuteValue != null)
                {
                    int inputValue = Convert.ToInt32(minuteValue);
                    if (inputValue > 0)
                    {
                        int hrs = 0;
                        int min = 0;

                        hrs = Math.DivRem(inputValue, 60, out min);
                        result = string.Format("{0}:{1}",
                            string.Format("{0:00}", hrs),
                            string.Format("{0:00}", min));
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static DateTime? ConvertMinuteToDateTime(int minuteValue)
        {
            DateTime? result = null;

            try
            {
                if (minuteValue > 0)
                {
                    int inputValue = Convert.ToInt32(minuteValue);
                    if (inputValue > 0)
                    {
                        int hrs = 0;
                        int min = 0;

                        hrs = Math.DivRem(inputValue, 60, out min);
                        result = Convert.ToDateTime(string.Format("{0}:{1}",
                            string.Format("{0:00}", hrs),
                            string.Format("{0:00}", min)));
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int ConvertHourStringToMinutes(string hourValue)
        {
            int result = 0;

            try
            {
                if (hourValue != null &&
                    hourValue.Contains(":"))
                {
                    string[] pieces = hourValue.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    int hourPart = Convert.ToInt32(pieces[0]);
                    int minutePart = Convert.ToInt32(pieces[1]);

                    result = hourPart * 60 + minutePart;
                }

                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string GetDateDescription(DateTime? dateInput)
        {
            string result = string.Empty;

            try
            {
                if (dateInput.HasValue)
                {
                    string dow = dateInput.Value.DayOfWeek.ToString();
                    result = string.Format("For Date: {0} ({1})",
                        dateInput.Value.ToLongDateString(),
                        dow);
                }

                return result.ToUpper();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertStringToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input.ToLower().Trim());
        }

        public static string ConvertStringToUpperCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            else
                return input.ToUpper().Trim();
        }
        #endregion
    }
}