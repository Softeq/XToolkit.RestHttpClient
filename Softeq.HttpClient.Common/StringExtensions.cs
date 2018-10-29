// Developed for LilBytes by Softeq Development Corporation
//

using System;

namespace Softeq.HttpClient.Common
{
    public static class StringExtensions
    {
        public static string EnsureStartsWith(this string targetStr, string startsWith)
        {
            return targetStr.StartsWith(startsWith, StringComparison.Ordinal) ? targetStr : $"{startsWith}{targetStr}";
        }

        public static string EnsureNotEndsWith(this string targetStr, string endsWith)
        {
            return targetStr.EndsWith(endsWith, StringComparison.Ordinal) ? targetStr.Substring(0, targetStr.Length - endsWith.Length) : targetStr;
        }
    }
}
