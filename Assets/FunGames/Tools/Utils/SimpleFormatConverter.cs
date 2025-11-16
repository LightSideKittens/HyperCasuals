using System;
using System.Globalization;
using UnityEngine;

namespace FunGames.Tools.Utils
{
    public class SimpleFormatConverter
    {
        public static int GetIntValue(string value, int defaultValue = -1)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception e)
            {
                LogException(e);
                return defaultValue;
            }
        }

        public static double GetDoubleValue(string value, int defaultValue = -1)
        {
            try
            {
                value = value.Replace(" ", String.Empty);
                if (!value.Contains(".")) value = value.Replace(",", ".");
                return Convert.ToDouble(value, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e)
            {
                LogException(e);
                return defaultValue;
            }
        }

        public static float GetFloat(string value,  int defaultValue = -1)
        {
            try
            {
                value = value.Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
                value = value.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, ",");
                return float.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e)
            {
                LogException(e);
                return defaultValue;
            }
        }

        public static bool GetBooleanValue(string value)
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception e)
            {
                if (value is "0" or "false") return false;
                if (value is "1" or "true") return true;

                LogException(e);
                return false;
            }
        }

        private static void LogException(Exception e)
        {
            Debug.LogWarning("[SimpleFormatConverter] failed to convert value : " + e);
        }
    }
}