using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace FunGames.Core
{
    public class FGLTVMeasurement
    {
        private const string PP_LTV = "fg_ltv";

        private static Action<double> _onNewRevenueTracked;
        public static event Action<double> OnNewRevenueTracked
        {
            add => _onNewRevenueTracked += value;
            remove => _onNewRevenueTracked -= value;
        }

        public static void AddRevenue(double revenue)
        {
            double currentLtv = GetLtvValue();
            PlayerPrefs.SetString(PP_LTV, (currentLtv + revenue).ToString(CultureInfo.InvariantCulture));

            StringBuilder sb = new StringBuilder();
            sb.Append("LTV Stored: ");
            sb.Append("- Last Stored LTV: " + currentLtv);
            sb.Append("- Current Revenue tracked: " + revenue);
            sb.Append("- New LTV stored: " + PlayerPrefs.GetString(PP_LTV));
            Debug.Log(sb.ToString());
            _onNewRevenueTracked?.Invoke(revenue);
        }

        public static string GetLtv()
        {
            return PlayerPrefs.GetString(PP_LTV,"0");
        }
        
        public static double GetLtvValue()
        {
            return Double.Parse(GetLtv(), CultureInfo.InvariantCulture);
        }
    }
}