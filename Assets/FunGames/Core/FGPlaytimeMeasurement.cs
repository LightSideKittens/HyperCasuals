using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace FunGames.Core
{
    public class FGPlaytimeMeasurement : FGSingleton<FGPlaytimeMeasurement>
    {
        private const string PP_TOTAL_PLAYTIME = "fg_total_playtime";

        private DateTime _lastStoredDate;

        private void Awake()
        {
            _lastStoredDate = DateTime.Now;
            StartCoroutine(CheckPlaytime());
        }

        public int GetTotalPlaytime()
        {
            return StoreTime();
        }

        private int StoreTime()
        {
            double secondsSinceLastTracked = Math.Abs((_lastStoredDate - DateTime.Now).TotalSeconds);
            double currentPlaytime =
                Double.Parse(PlayerPrefs.GetString(PP_TOTAL_PLAYTIME, "0"), CultureInfo.InvariantCulture);
            double newPlaytime = currentPlaytime + secondsSinceLastTracked;
            PlayerPrefs.SetString(PP_TOTAL_PLAYTIME, newPlaytime.ToString(CultureInfo.InvariantCulture));
            _lastStoredDate = DateTime.Now;
            int outputValue = (int)Math.Round(newPlaytime, MidpointRounding.AwayFromZero);
            StringBuilder sb = new StringBuilder();
            sb.Append("Playtime Stored: ");
            sb.Append("- Last Stored Playtime: " + currentPlaytime);
            sb.Append("- Seconds Since Last Tracked: " + secondsSinceLastTracked);
            sb.Append("- New Playtime stored: " + PlayerPrefs.GetString(PP_TOTAL_PLAYTIME));
            sb.Append("- Output: " + outputValue);
            // Debug.Log(sb.ToString());
            return outputValue;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) _lastStoredDate = DateTime.Now;
            else StoreTime();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) StoreTime();
            else _lastStoredDate = DateTime.Now;
        }

        private void OnApplicationQuit()
        {
            StoreTime();
        }

        private IEnumerator CheckPlaytime()
        {
            while (true)
            {
                StoreTime();
                yield return new WaitForSeconds(10);
            }
        }
    }
}