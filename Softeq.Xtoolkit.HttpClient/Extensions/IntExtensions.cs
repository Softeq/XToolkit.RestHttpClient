
namespace Softeq.XToolkit.HttpClient.Extensions
{
    internal static class IntExtensions
    {
        public static bool IsIntInRange(this int target, int rangeStart, int rangeEnd)
        {
            return target >= rangeStart && target <= rangeEnd;
        }
    }
}
