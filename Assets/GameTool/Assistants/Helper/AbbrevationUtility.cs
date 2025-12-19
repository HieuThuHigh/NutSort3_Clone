using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameTool.Assistants.Helper
{
    public static class AbbrevationUtility
    {
        private static readonly SortedDictionary<long, string> abbrevations = new SortedDictionary<long, string>
        {
            {1000,"K"},
            {1000000, "M" },
            {1000000000, "B" },
            {1000000000000, "T"},
            {1000000000000000, "q"}
        };

        public static string AbbreviateNumber(long number)
        {
            for (int i = abbrevations.Count - 1; i >= 0; i--)
            {
                KeyValuePair<long, string> pair = abbrevations.ElementAt(i);
                if (Mathf.Abs(number) >= pair.Key)
                {
                    long roundedNumber = number / pair.Key;
                    long rounded2Number = (number % pair.Key) / (pair.Key / 10);
                    return roundedNumber.ToString() + "." + rounded2Number + pair.Value;
                }
            }
            return number.ToString();
        }

        public static string AbbrevationTimeHMS(float time)
        {
            float hours = Mathf.FloorToInt(time / (60 * 60));
            float minutes = Mathf.FloorToInt((time - hours * 60 * 60) / 60);
            float seconds = Mathf.FloorToInt((time - hours * 60 * 60) % 60);
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        public static string AbbrevationTimeAuto(float time)
        {
            float hours = Mathf.FloorToInt(time / (60 * 60));
            float minutes = Mathf.FloorToInt((time - hours * 60 * 60) / 60);
            float seconds = Mathf.FloorToInt((time - hours * 60 * 60) % 60);
            if (time > 60 * 60)
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public static float RoundToFloatDecimal(float value, int decimalValue)
        {
            string text = "1";
            float roundValue = 1;
            float roundDecimal = 1;

            if (decimalValue <= 0)
            {
                decimalValue = 0;
            }

            for(int i = 0; i < decimalValue; i++)
            {
                text += "0";
            }

            roundValue = float.Parse(text);

            if(roundValue > 1)
            {
                roundDecimal = roundValue / (roundValue * 10f);
            }

            return Mathf.Round(value * roundValue) * roundDecimal;
        }
    }
}