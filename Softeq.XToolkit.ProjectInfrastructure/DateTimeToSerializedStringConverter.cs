// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Globalization;

namespace Softeq.XToolkit.CrossCutting
{
    public static class DateTimeToSerializedStringConverter
    {
        private static readonly IFormatProvider DateTimeFormatProvider = CultureInfo.InvariantCulture;

        public static string DateToString(DateTime dateTime)
        {
            return dateTime.ToString("o", DateTimeFormatProvider);
        }

        public static DateTime? StringToDate(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            if (DateTime.TryParse(dateTimeString, DateTimeFormatProvider, DateTimeStyles.AdjustToUniversal, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
