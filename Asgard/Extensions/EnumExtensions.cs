using System;

namespace Asgard.Extensions
{
    public static class EnumExtensions
    {
        //public static T? Get<T>(string s) where T : struct, Enum =>
        //    Enum.TryParse(s, true, out T value) ? value : null;

        public static T? Get<T>(this string s) where T : struct, Enum =>
            Enum.TryParse(s, true, out T value) ? value : null;
    }
}
