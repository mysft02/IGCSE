using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public static class EnumHelper
    {
        public static string Name<T>(this T srcValue) => GetCustomName(typeof(T).GetField(srcValue?.ToString() ?? string.Empty));
        private static string GetCustomName(FieldInfo? fi)
        {
            Type type = typeof(CustomName);

            Attribute? attr = null;
            if (fi is not null)
            {
                attr = Attribute.GetCustomAttribute(fi, type);
            }

            return (attr as CustomName)?.Name ?? string.Empty;
        }
    }
}
