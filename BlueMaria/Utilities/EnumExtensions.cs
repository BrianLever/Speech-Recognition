using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

// part of NSDK utilities

namespace BlueMaria.Utilities
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum @enum)
        {
            Type enumType = @enum.GetType();
            string value = @enum.ToString();
            FieldInfo info = enumType.GetField(@enum.ToString());
            if (info != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    value = attributes[0].Description;
            }
            return value;
        }

        public static string EnumToDescription(this object @enum)
        {
            Type enumType = @enum.GetType();
            string value = @enum.ToString();
            FieldInfo info = enumType.GetField(@enum.ToString());
            if (info != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    value = attributes[0].Description;
            }
            return value;
        }

        private static void CheckIsEnum<T>(bool checkIfFlags = true)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
            if (checkIfFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
        }

        public static bool IsFlagSet<T>(this T value, T flag) where T : struct
        {
            CheckIsEnum<T>(true);
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static bool IsFlagSet<T>(this T value, T flag, bool checkIfFlags = true) where T : struct
        {
            CheckIsEnum<T>(checkIfFlags); // true);
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static bool AreAllFlagsSet<T>(this T value, T flag, bool checkIfFlags = true) where T : struct
        {
            CheckIsEnum<T>(checkIfFlags); // true);
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) == lFlag; // != 0;
        }

        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct
        {
            CheckIsEnum<T>(true);
            foreach (T flag in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (value.IsFlagSet(flag))
                    yield return flag;
            }
        }

        public static T SetFlags<T>(this T value, T flags, bool on) where T : struct
        {
            CheckIsEnum<T>(true);
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flags);
            if (on)
            {
                lValue |= lFlag;
            }
            else
            {
                lValue &= (~lFlag);
            }
            return (T)Enum.ToObject(typeof(T), lValue);
        }

        public static T SetFlags<T>(this T value, T flags) where T : struct
        {
            return value.SetFlags(flags, true);
        }

        public static T ClearFlags<T>(this T value, T flags) where T : struct
        {
            return value.SetFlags(flags, false);
        }

        public static T CombineFlags<T>(this IEnumerable<T> flags) where T : struct
        {
            CheckIsEnum<T>(true);
            long lValue = 0;
            foreach (T flag in flags)
            {
                long lFlag = Convert.ToInt64(flag);
                lValue |= lFlag;
            }
            return (T)Enum.ToObject(typeof(T), lValue);
        }

        public static string GetDescription<T>(this T value) where T : struct
        {
            CheckIsEnum<T>(false);
            string name = Enum.GetName(typeof(T), value);
            if (name != null)
            {
                FieldInfo field = typeof(T).GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        public static string ToMultiString<T>(this T value) where T : struct
        {
            CheckIsEnum<T>(false);
            string type = typeof(T).Name;
            return string.Join(" | ", value.ToString().Split(',').Select(x => type + "." + x.Trim()));
            //var strvalue = value.ToString();
            //var values = strvalue.Split(',');
            //if (values.Length <= 1) return strvalue;
            //return string.Join(" | ", values.Select(x => type + "." + x.Trim()));
            //// return null;
        }

    }
}
