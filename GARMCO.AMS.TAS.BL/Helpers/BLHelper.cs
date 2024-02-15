using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Helpers
{
    public class BLHelper
    {
        #region Enumeration                
        public enum SwipeCodes
        {
            MAINGATE,
            WORKPLACE
        }

        public enum SwipeTypes
        {
            IN,
            OUT,
            Unknown
        }

        public enum SwipeTimeTypes
        {
            MainGateSwipeIn,
            MainGateSwipeOut,
            WorkplaceSwipeIn,
            WorkplaceSwipeOut
        }

        public enum SaveType
        {
            NotDefined,
            Insert,
            Update,
            Delete,
            Others
        }

        public enum WorkflowProgressStatuses
        {
            Completed = 109,
            ByPassed = 108,
            InProgress = 107,
            Pending = 106
        }

        public enum WorkflowActivityTypes
        {
            ACTION,
            CONDITION,
            FUNCTION,
            SENDEMAIL,
            NOACTION
        }

        public enum SwipeCode
        {
            MANUAL,
            MAINGATE,
            WORKPLACE
        }

        public enum AttendeeType
        {
            Employee,
            Contractor
        }

        public enum ArrivalStatus
        {
            Ontime,
            Late,
            LeftEarly,
            LateAndLeftEarly,
            Dayoff,
            Holiday,
            MissingSwipe,
            Absent,
            OnLeave,
            WorkExtraHours,
            DIL
        }
        #endregion

        #region Constants

        #region Attendance Dashboard Status Constants
        public const string CONST_ARRIVAL_NORMAL = "i";
        public const string CONST_ARRIVAL_LATE = "l";
        public const string CONST_LEFT_NORMAL = "o";
        public const string CONST_LEFT_EARLY = "e";
        public const string CONST_NOT_COME_YET = "x";
        public const string CONST_MANUAL_IN = "im";
        public const string CONST_MANUAL_OUT = "om";

        public const string CONST_ARRIVAL_NORMAL_ICON = "~/Images/ArrivalNormal.ICO";
        public const string CONST_ARRIVAL_LATE_ICON = "~/Images/ArrivalLate.png";
        public const string CONST_LEFT_NORMAL_ICON = "~/Images/LeftNormal.ico";
        public const string CONST_LEFT_EARLY_ICON = "~/Images/LeftEarly.png";
        public const string CONST_NOT_COME_YET_ICON = "~/Images/NotComeYet.ICO";
        public const string CONST_MANUAL_IN_ICON = "~/Images/InManual.ICO";
        public const string CONST_MANUAL_OUT_ICON = "~/Images/OutManual.png";
        public const string CONST_NO_EMPLOYEE_PHOTO = "~/Images/no_photo_icon.png";

        public const string CONST_ARRIVAL_NORMAL_NOTES = "Arrived on-time";
        public const string CONST_ARRIVAL_LATE_NOTES = "Arrived late";
        public const string CONST_LEFT_NORMAL_NOTES = "Left on-time";
        public const string CONST_LEFT_EARLY_NOTES = "Left early";
        public const string CONST_NOT_COME_YET_NOTES = "Not attended to work";
        public const string CONST_MANUAL_IN_NOTES = "Manually logged the time-in";
        public const string CONST_MANUAL_OUT_NOTES = "Manually logged the time-out";
        #endregion

        #region General Constants                
        public const string CONST_DEFAULT_PHOTO = "~/Images/fireteam_icon.jpg";
        public const string CONST_EMP_PRESENT_ICON = "~/Images/check_icon.png";
        public const string CONST_EMP_ABSENT_ICON = "~/Images/cross_icon.png";
        public const string CONST_NO_PHOTO_MESSAGE = "Unable to display the employee photo due to access restriction or the image is not available.";
        public const string CONST_EMP_PRESENT_NOTES = "Employee is present in the company";
        public const string CONST_EMP_ABSENT_NOTES = "Employee is not present in the company";
        public const string STATUS_HANDLING_CODE_APPROVED = "Approved";
        public const string STATUS_HANDLING_CODE_CANCELLED = "Cancelled";
        public const string STATUS_HANDLING_CODE_CLOSED = "Closed";
        public const string STATUS_HANDLING_CODE_OPEN = "Open";
        public const string STATUS_HANDLING_CODE_REJECTED = "Rejected";
        public const string STATUS_HANDLING_CODE_VALIDATED = "Validated";
        #endregion

        #endregion

        #region Public Methods
        public static string GetUserFirstName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return string.Empty;

            try
            {
                if (userName.ToUpper().Trim() == "WATER TREATMENT TERMINAL")
                {
                    return userName;
                }

                string result = string.Empty;
                Match m = Regex.Match(userName, @"(\w*) (\w.*)");
                string firstName = m.Groups[1].ToString();

                if (!string.IsNullOrEmpty(firstName))
                {
                    System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
                    result = textInfo.ToTitleCase(firstName.ToLower().Trim());
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static long ConvertObjectToLong(object value)
        {
            long result;
            if (value != null && long.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static int ConvertObjectToInt(object value)
        {
            int result;
            if (value != null && int.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static byte ConvertObjectToByte(object value)
        {
            byte result;
            if (value != null && byte.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static double ConvertObjectToDouble(object value)
        {
            double result;
            if (value != null && double.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static decimal ConvertObjectToDecimal(object value)
        {
            decimal result;
            if (value != null && decimal.TryParse(value.ToString(), out result))
                return result;
            else
                return 0;
        }

        public static bool ConvertObjectToBolean(object value)
        {
            bool result;
            if (value != null && bool.TryParse(value.ToString(), out result))
                return result;
            else
                return false;
        }

        public static bool ConvertNumberToBolean(object value)
        {
            if (value != null && Convert.ToInt32(value) == 1)
                return true;
            else
                return false;
        }

        public static DateTime? ConvertObjectToDate(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DateTime ConvertObjectToRealDate(object value)
        {
            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
                }

                DateTime result;
                if (value != null && DateTime.TryParse(value.ToString(), out result))
                {
                    return result;
                }
                else
                    return DateTime.Parse("01/01/1900 00:00:00");
            }
            catch (Exception)
            {
                return DateTime.Parse("01/01/1900 00:00:00");
            }
        }

        public static string ConvertObjectToString(object value)
        {
            return value != null ? value.ToString().Trim() : string.Empty;
        }

        public static string ConvertObjectToDateString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("dd-MMM-yyyy");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertObjectToUSDateString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-US")
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("MM/dd/yyyy");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertObjectToBritishDateString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("dd/MM/yyyy");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertObjectToDateTimeString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Trim() != "en-GB")
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("dd-MMM-yyyy hh:mm tt");
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertStringToTitleCase(string input)
        {
            System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        public static string ConvertObjectToTimeString(object value)
        {
            DateTime result;

            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Trim() != "en-GB")
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                }

                if (value != null && DateTime.TryParse(value.ToString(), out result))
                    return result.ToString("HH:mm:ss");
                else
                    return string.Empty;
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
        #endregion
    }
}
