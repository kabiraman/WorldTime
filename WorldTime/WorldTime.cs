/* 
 * Name: WorldTime.cs
 * Programmed by: Karthik Abiraman (kmabiraman@gmail.com)
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace WorldTime
{
    /// <summary>
    /// This class contains the complete logic for calculating the date and 
    /// time for all the cities present in the database.
    /// </summary>
    static class WorldTime
    {
        /// <summary>
        /// This Canada-specific CultureInfo is used when using 
        /// DateTime.Parse() to ensure database date values are 
        /// handled in a standard way.
        /// </summary>
        private static System.Globalization.CultureInfo s_culture =
            System.Globalization.CultureInfo.CreateSpecificCulture(
            "en-CA");

        /// <summary>
        /// Calculates time for a city.
        /// </summary>
        /// <param name="strTargetOffset">
        /// The hours the target city is ahead or behind GMT.
        /// </param>
        /// <param name="intTargetHasDST">
        /// Whether the city has DST or not.
        /// </param>
        /// <param name="strTargetDSTStart">
        /// The DST start date and time for the city.
        /// </param>
        /// <param name="strTargetDSTEnd">
        /// The DST end date and time for the city.
        /// </param>
        /// <param name="dtNow">The current system time.</param>
        /// <param name="strSourceOffset">
        /// The hours the source city is ahead or behind GMT.
        /// </param>
        /// <returns>The calculated time.</returns>
        public static string GetTime(string strTargetOffset, int intTargetHasDST, 
            string strTargetDSTStart, string strTargetDSTEnd, DateTime dtNow, 
            string strSourceOffset, string strDateFormat, 
            string strTimeFormat)
        {

            string strReturnValue = "";
            DateTime dtReturnValue;
            DateTime dtGMT;
            double dblSourceOffset = 0;
            double dblTargetOffset = 0;

            dblSourceOffset = double.Parse(strSourceOffset);
            dblTargetOffset = double.Parse(strTargetOffset);
            
            // calculate the GMT time
            dtGMT = dtNow.AddHours((-dblSourceOffset));

            // the city has no end to DST, simply set it to end of year
            if (intTargetHasDST == 1 && String.IsNullOrEmpty(strTargetDSTEnd)) strTargetDSTEnd = "31/12 23:59";

            // if the target city has DST
            if (intTargetHasDST == 1)
            {
                // extract the date part from the target city's DST start and add
                // the current year to it
                string strDatePart = strTargetDSTStart.Substring(0, 5) + "/" + 
                    dtNow.Year;
                
                // extract the time part from the target city's DST start
                string strTimePart = strTargetDSTStart.Substring(6);

                // we now have the date and time for DST start in the proper format
                DateTime dtTargetDSTStart = 
                    DateTime.Parse(strDatePart + " " + strTimePart, s_culture);

                // extract the date part from the target city's DST end and add
                // the current year to it
                strDatePart = strTargetDSTEnd.Substring(0, 5) + "/" + dtNow.Year;
                
                // extract the time part from the target city's DST end
                strTimePart = strTargetDSTEnd.Substring(6);

                // we now have the date and time for DST end in the proper format
                DateTime dtTargetDSTEnd = 
                    DateTime.Parse(strDatePart + " " + strTimePart, s_culture);

                // if the DST start is greater than DST end, change the year for 
                // DST end to current year + 1
                if (dtTargetDSTStart.CompareTo(dtTargetDSTEnd) == 1)
                {
                    strDatePart = strTargetDSTEnd.Substring(0, 5) + "/" + 
                        (dtNow.Year + 1);
                    strTimePart = strTargetDSTEnd.Substring(6);

                    dtTargetDSTEnd = 
                        DateTime.Parse(strDatePart + " " + strTimePart, s_culture);
                }

                // equal = 0, greater than = 1, less than = -1
                // note: DST start and end are in GMT time
                if (dtGMT.CompareTo(dtTargetDSTStart) == 0 || 
                    dtGMT.CompareTo(dtTargetDSTEnd) == 0)
                {
                    dblTargetOffset += 1;
                }
                else if (dtGMT.CompareTo(dtTargetDSTStart) == 1 &&
                    dtGMT.CompareTo(dtTargetDSTEnd) == -1)
                {
                    dblTargetOffset += 1;
                }

                dtReturnValue = dtGMT.AddHours(dblTargetOffset);                
            }
            else
            {
                dtReturnValue = dtGMT.AddHours(dblTargetOffset);
            }

            if (Properties.Settings.Default.DateFormat == "WorldTime")
            {
                strReturnValue = dtReturnValue.DayOfWeek + ", " +
                    FormatMonth(dtReturnValue.Month) + " " +
                    dtReturnValue.Day + ", " + dtReturnValue.Year.ToString() +
                    " ";
            }
            else if (Properties.Settings.Default.DateFormat ==
                "SystemCultureLong")
            {
                strReturnValue = dtReturnValue.ToLongDateString() + " ";
            }
            else
            {
                strReturnValue = dtReturnValue.ToShortDateString() + " ";
            }

            strReturnValue += dtReturnValue.ToString("T",
                System.Globalization.CultureInfo.CreateSpecificCulture(
                strTimeFormat));

            return strReturnValue;
        }

        /// <summary>
        /// Calculates the current offset, be it DST start or end.
        /// </summary>
        /// <param name="strTargetOffset">
        /// The hours the target city is ahead or behind.
        /// </param>
        /// <param name="intTargetHasDST">
        /// Whether the city has DST or not.
        /// </param>
        /// <param name="strTargetDSTStart">
        /// The DST start date and time for the city.
        /// </param>
        /// <param name="strTargetDSTEnd">
        /// The DST end date and time for the city.
        /// </param>
        /// <returns>The calculated offset.</returns>
        public static string GetCurrentOffset(
            string strTargetOffset, int intTargetHasDST,
            string strTargetDSTStart, string strTargetDSTEnd, bool shouldFormatOffset)
        {
            return GetCurrentOffset(
                strTargetOffset, intTargetHasDST, strTargetDSTStart, strTargetDSTEnd, DateTime.Now, shouldFormatOffset);
        }

        public static string GetCurrentOffset(
            string strTargetOffset, int intTargetHasDST,
            string strTargetDSTStart, string strTargetDSTEnd, DateTime dtTargetDate, bool shouldFormatOffset)
        {
            string strReturnValue = "";
            DateTime dtGMT;
            double dblThisPCOffset = 0;
            double dblTargetOffset = 0;

            // if the target city has no DST, no calculation is required, 
            // simply format the offset and return it
            if (intTargetHasDST == 1 && !String.IsNullOrEmpty(strTargetDSTEnd))
            {
                dblTargetOffset = double.Parse(strTargetOffset);

                // get the system offset
                dblThisPCOffset = TimeZone.CurrentTimeZone.GetUtcOffset
                    (dtTargetDate).TotalHours;

                // calculate the GMT
                dtGMT = dtTargetDate.AddHours((-dblThisPCOffset));

                // extract the date part
                string strDatePart = strTargetDSTStart.Substring(0, 5) + "/" +
                    dtTargetDate.Year;

                // extract the time part
                string strTimePart = strTargetDSTStart.Substring(6);

                // we now have the date and time for DST start in the proper format
                DateTime dtTargetDSTStart =
                    DateTime.Parse(strDatePart + " " + strTimePart, s_culture);

                // extract the date part
                strDatePart = strTargetDSTEnd.Substring(0, 5) + "/" + dtTargetDate.Year;

                // extract the time part
                strTimePart = strTargetDSTEnd.Substring(6);

                // we now have the date and time for DST end in the proper format
                DateTime dtTargetDSTEnd =
                    DateTime.Parse(strDatePart + " " + strTimePart, s_culture);

                // if DST start is greater than DST end, change the year for DST 
                // end to year + 1
                if (dtTargetDSTStart.CompareTo(dtTargetDSTEnd) == 1)
                {
                    strDatePart = strTargetDSTEnd.Substring(0, 5) + "/" +
                        (dtTargetDate.Year + 1);
                    strTimePart = strTargetDSTEnd.Substring(6);

                    dtTargetDSTEnd =
                        DateTime.Parse(strDatePart + " " + strTimePart, s_culture);
                }

                // equal = 0, greater than = 1, less than = -1
                if (dtGMT.CompareTo(dtTargetDSTStart) == 0 ||
                    dtGMT.CompareTo(dtTargetDSTEnd) == 0)
                {
                    dblTargetOffset += 1;
                }
                else if (dtGMT.CompareTo(dtTargetDSTStart) == 1 &&
                    dtGMT.CompareTo(dtTargetDSTEnd) == -1)
                {
                    dblTargetOffset += 1;
                }

                // we need to perform formatting of the offset
                if (dblTargetOffset != double.Parse(strTargetOffset))
                {
                    string strHour = strTargetOffset.Substring(1, 2);
                    int intHour = int.Parse(strHour);

                    if (strTargetOffset.IndexOf("-") == -1)
                    {
                        intHour += 1;
                    }
                    else
                    {
                        intHour -= 1;
                    }

                    strHour = intHour.ToString();

                    if (strHour.Length == 1)
                    {
                        strTargetOffset = strTargetOffset.Substring(0, 1) + "0" +
                            intHour.ToString() + strTargetOffset.Substring(3);
                    }
                    else
                    {
                        strTargetOffset = strTargetOffset.Substring(0, 1) +
                            intHour.ToString() + strTargetOffset.Substring(3);
                    }
                }
            }

            if (shouldFormatOffset)
            {
                strReturnValue = FormatOffset(strTargetOffset);
            }
            else
            {
                strReturnValue = strTargetOffset;
            }

            return strReturnValue;
        }

        /// <summary>
        /// Formats the offset to an appropriate format to display to the user.
        /// </summary>
        /// <param name="strOffset">The offset to format.</param>
        /// <returns>The formatted offset.</returns>
        public static string FormatOffset(string strOffset)
        {
            string strHourPart = "";
            int intOffsetLength = strOffset.Length;

            strHourPart = strOffset.Substring(4);
            
            switch (strHourPart)
            {
                case "25":
                    strHourPart = "15";
                    break;
                case "50":
                    strHourPart = "30";
                    break;
                case "75":
                    strHourPart = "45";
                    break;
            }

            strOffset = strOffset.Substring(0, 4) + strHourPart;

            return strOffset;
        }

        /// <summary>
        /// Formats the DST start/end appropriate for display to the user.
        /// </summary>
        /// <param name="strDST">DST start or end date and time.</param>
        /// <param name="strOffset">The hours ahead or behind GMT.</param>
        /// <param name="bIsStart">
        /// Whether the DST passed is the start or end.
        /// </param>
        /// <returns>The formatted DST.</returns>
        public static string FormatDST(string strDST, string strOffset, 
            bool bIsStart)
        {
            string strDatePart = strDST.Substring(0, 5) + "/" + 
                DateTime.Now.Year;
            string strTimePart = strDST.Substring(6);

            DateTime dtDST =
                DateTime.Parse(strDatePart + " " + strTimePart, s_culture);

            dtDST = dtDST.AddHours(double.Parse(strOffset));

            if (!bIsStart)
            {
                dtDST = dtDST.AddHours(1);
            }

            return dtDST.DayOfWeek + ", " + FormatMonth(dtDST.Month) + " " +
                dtDST.Day + ", " + dtDST.ToLongTimeString();            
        }

        /// <summary>
        /// Formats the month from an integer value to its name.
        /// </summary>
        /// <param name="intMonth">The number of the month.</param>
        /// <returns>The formatted month (name).</returns>
        internal static string FormatMonth(int intMonth)
        {
            string strMonth = "";

            switch (intMonth)
            {
                case 1:
                    strMonth = "January";
                    break;
                case 2:
                    strMonth = "February";
                    break;
                case 3:
                    strMonth = "March";
                    break;
                case 4:
                    strMonth = "April";
                    break;
                case 5:
                    strMonth = "May";
                    break;
                case 6:
                    strMonth = "June";
                    break;
                case 7:
                    strMonth = "July";
                    break;
                case 8:
                    strMonth = "August";
                    break;
                case 9:
                    strMonth = "September";
                    break;
                case 10:
                    strMonth = "October";
                    break;
                case 11:
                    strMonth = "November";
                    break;
                case 12:
                    strMonth = "December";
                    break;
            }

            return strMonth;
        }
    }

}