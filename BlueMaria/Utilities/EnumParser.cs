using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using NSDK.Extensions;
//using NSDK.Linq.Extensions;

// part of NSDK utilities

namespace BlueMaria.Utilities 
{
    public class EnumParser
    {
        public enum Error { Success = 0, Empty = 1, ParseError = 2, NotFound = 3, }

        #region int
        public static TEnum Parse<TEnum>(int ivalue)
        where TEnum : struct
        {
            Error error;
            TEnum novalue = default(TEnum);
            return Parse<TEnum>(ivalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(int ivalue, TEnum novalue)
        where TEnum : struct
        {
            Error error;
            return Parse<TEnum>(ivalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(int ivalue, TEnum novalue, out Error error)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            TEnum value = novalue;
            error = Error.ParseError;
            try
            {
                value = (TEnum)Enum.ToObject(typeof(TEnum), ivalue);// as TEnum;
                error = Error.Success;
                /// there is no empty status
                //error = error.Empty;
            }
            catch
            {
                error = Error.ParseError;
            }
            return value;
        }
        #endregion

        #region uint
        public static TEnum Parse<TEnum>(uint ivalue)
        where TEnum : struct
        {
            Error error;
            TEnum novalue = default(TEnum);
            return Parse<TEnum>(ivalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(uint ivalue, TEnum novalue)
        where TEnum : struct
        {
            Error error;
            return Parse<TEnum>(ivalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(uint ivalue, TEnum novalue, out Error error)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            TEnum value = novalue;
            error = Error.ParseError;
            try
            {
                value = (TEnum)Enum.ToObject(typeof(TEnum), ivalue);// as TEnum;
                error = Error.Success;
                /// there is no empty status
                //error = error.Empty;
            }
            catch
            {
                error = Error.ParseError;
            }
            return value;
        }
        #endregion

        #region char
        public static TEnum Parse<TEnum>(char ivalue, TEnum novalue, out Error error)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            TEnum value = novalue;
            error = Error.ParseError;
            try
            {
                value = (TEnum)Enum.ToObject(typeof(TEnum), ivalue);// as TEnum;
                error = Error.Success;
                /// there is no empty status
                //error = error.Empty;
            }
            catch
            {
                error = Error.ParseError;
            }
            return value;
        }
        #endregion

        #region string
        public static TEnum Parse<TEnum>(string strvalue)
        where TEnum : struct
        {
            Error error;
            TEnum novalue = default(TEnum);
            return Parse<TEnum>(strvalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(string strvalue, TEnum novalue)
        where TEnum : struct
        {
            Error error;
            return Parse<TEnum>(strvalue, novalue, out error);
        }

        public static TEnum Parse<TEnum>(string strvalue, TEnum novalue, out Error error)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            TEnum value = novalue;
            error = Error.ParseError;

            if (strvalue == "Not Found")
            {
                error = Error.NotFound;
                return value;
            }
            try
            {
                if (!string.IsNullOrEmpty(strvalue))
                {
                    value = (TEnum)Enum.Parse(typeof(TEnum), strvalue, true);
                    error = Error.Success;
                }
                else
                    error = Error.Empty;
            }
            catch//( Exception e )
            {
                error = Error.ParseError;
            }
            return value;
        }

        public static bool TryParse<TEnum>(string strvalue, out TEnum value)
        where TEnum : struct
        {
            Error error;
            return TryParse(strvalue, default(TEnum), out error, out value);
        }

        public static bool TryParse<TEnum>(string strvalue, TEnum novalue, out Error error, out TEnum value)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            value = novalue;
            error = Error.ParseError;

            if (strvalue == "Not Found")
            {
                error = Error.NotFound;
                return false; // value;
            }
            try
            {
                if (!string.IsNullOrEmpty(strvalue))
                {
                    if (Enum.TryParse<TEnum>(strvalue, true, out value))
                    {
                        error = Error.Success;
                        return true;
                    }
                    else
                        error = Error.ParseError;
                }
                else
                    error = Error.Empty;
            }
            catch//( Exception e )
            {
                error = Error.ParseError;
            }
            return false; // value;
        }

        public static TEnum ParseDescription<TEnum>(string strvalue, TEnum novalue)
            where TEnum : struct
        {
            Error error;
            TEnum value;
            if (!ParseDescription(strvalue, novalue, out error, out value)) return novalue;
            return value;
        }
        public static bool ParseDescription<TEnum>(string strvalue, out TEnum value)
            where TEnum : struct
        {
            Error error;
            return ParseDescription(strvalue, default(TEnum), out error, out value);
        }

        public static bool ParseDescription<TEnum>(string strvalue, TEnum novalue, out Error error, out TEnum value)
        where TEnum : struct
        {
            Ensure.Is.True(typeof(TEnum).IsEnum);
            value = novalue;
            error = Error.ParseError;

            if (strvalue == "Not Found")
            {
                error = Error.NotFound;
                return false; // value;
            }
            try
            {
                if (!string.IsNullOrEmpty(strvalue))
                {
                    var values = ((TEnum[])Enum.GetValues(typeof(TEnum)))
                        .Where(x => EnumExtensions.EnumToDescription(x).ToLower() == strvalue.ToLower())
                        .ToArray();
                    if (values.Any())
                    {
                        value = values.FirstOrDefault();
                        error = Error.Success;
                        return true;
                    }
                    if (Enum.TryParse<TEnum>(strvalue, true, out value))
                    {
                        error = Error.Success;
                        return true;
                    }
                    else
                        error = Error.ParseError;
                }
                else
                    error = Error.Empty;
            }
            catch//( Exception e )
            {
                error = Error.ParseError;
            }
            return false; // value;
        }

        public static bool Exists<TEnum>(string strvalue)
        where TEnum : struct
        {
            Error error;
            TEnum value = Parse<TEnum>(strvalue, default(TEnum), out error);
            return (error == Error.Success);
        }

        #endregion

    }

}
