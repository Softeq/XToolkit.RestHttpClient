// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Softeq.XToolkit.HttpClient.Extensions
{
    internal static class EnumExtensions
    {
        public static TEnum GetPrevious<TEnum>(this TEnum target)
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

            var foundIndex = values.IndexOf(target);

            return foundIndex == 0 ? values.Last() : values[foundIndex - 1];
        }

        public static IEnumerable<TEnum> GetValues<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
        }

        public static TEnum RemoveFlag<TEnum>(this Enum type, TEnum enumFlag)
        {
            try
            {
                return (TEnum)(object)((int)(object)type & ~(int)(object)enumFlag);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Could not remove flag value {enumFlag} from enum {typeof(TEnum).Name}", ex);
            }
        }

        public static bool In<TEnum>(this TEnum target, params TEnum[] values)
        {
            return values.Contains(target);
        }
    }
}
