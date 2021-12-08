using System;
//using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
//using System.ComponentModel;


// part of NSDK utilities

namespace BlueMaria.Utilities
{
    /// <summary>
    /// This is my (Dragan's) class for conversion/formatting - and lib with some handy utils I'm using 
    /// TODO: we should move/merge some of this (I'm sure it exists in some form just no time to look for it)
    /// </summary>
	public sealed class Ensure
	{
		public static readonly Ensure I = new Ensure();
        public static Ensure Is { get { return I; } }
        public static Ensure Try { get { return I; } }
		private Ensure(){}
		[Conditional("DEBUG")]//_FIX")]
		public void /*ThatAre*/TheSame( object value1, object value2 )
		{
			if( value1!=value2 )
				throw new ArgumentException();
		}
		[Conditional("DEBUG")]//_FIX")]
		public void OfSameType( object value1, object value2 )
		{
			if( value1.GetType()!=value2.GetType() )
				throw new ArgumentException();
		}
		//[Conditional("DEBUG")]//_FIX")]
		//public void True( bool condition ) {if( !condition ) throw new ArgumentException();}
		[Conditional("DEBUG")]//_FIX")]
		public void /*ConditionIs*/True( bool condition, string errormessage )
		{
			if( !condition )
			{
				Exception e = new ArgumentException(errormessage);
				//Log.I.Error(e);
				throw e;
			}
		}
		public void True( string errormessage, params bool[] conditions )
		{
			foreach( bool condition in conditions )
				if( !condition )
					throw new ArgumentException( errormessage );
		}
		public void True( params bool[] conditions )
		{
			foreach( bool condition in conditions )
				if( !condition )
					throw new ArgumentException();
		}
		[Conditional("DEBUG")]
		public void TrueTest(params bool[] conditions)
		{
			foreach (bool condition in conditions)
				if (!condition)
					throw new ArgumentException();
		}
		public bool True(bool dothrow, params bool[] conditions)
		{
			foreach (bool condition in conditions)
			{
				if (!condition)
				{
					if (dothrow)
						throw new ArgumentException();
					return false;
				}
			}
			return true;
		}
		[Conditional("DEBUG")]//_FIX")]
		public void NotNull( object value )
		{
			if( value==null )
			{
				Exception e = new ArgumentNullException();
				//Log.I.Error(e);
				throw e;
			}
			//throw new ArgumentNullException();//"info");
			//if( value==null ) throw new ArgumentException();
		}
		public string NoneEmptyString( string value )
		{
			return NoneEmptyString( value, null );
		}
		public string NoneEmptyString( string value, string defaultval )
		{
			return value!=""? value : defaultval;
		}
		public string ValidNoneEmptyString( string value, string defaultval )
		{
			return (value!=null && value!="")? value : defaultval;
		}
		public string ValidNoneEmptyString( string value, string delimiter, string defaultstr )
		{
			return (value!=null && value!="")? (value+delimiter) : defaultstr;
		}
		public string PadString( string value, int len, char delimiter )
		{
			return (value!=null && value!="")? (value.PadRight( len, delimiter)) : "";
		}
		public string ValidNoneEmptyString( string value, string pre, string post, string defaultstr )
		{
			return (value!=null && value!="")? (pre+value+post) : defaultstr;
		}
		public string ValidString( string value )
		{
			return ValidString( value, "" );
		}
		public string ValidString( string value, string defaultstr )
		{
			return value!=null? value : defaultstr;
		}
		public DateTime ValidDateTime( object value, DateTime defaultdate )
		{
			if( value==System.DBNull.Value || value==null || !(value is DateTime) )
				return defaultdate;
			return (DateTime)value;
		}
		public string ValidDateString( object value, string defaultstr )
		{
			if( value==System.DBNull.Value || value==null || !(value is DateTime) )
				return defaultstr;
			return ((DateTime)value).ToShortDateString();
		}
		static readonly DateTime _minDbValue = new DateTime(1753,1,1,0,0,0,0);
		//static readonly DateTime _maxDbValue = new DateTime(1753,1,1,0,0,0,0);
		public DateTime ValidDate( DateTime value, DateTime? defaultDate = null  )
		{
            var defaultdt = defaultDate ?? _minDbValue + new TimeSpan(1, 0, 0);
            return value <= _minDbValue || value >= DateTime.MaxValue ?
                defaultdt : value;
			//return value!=null? value : "";
		}
		public int ValidInt32( string value )
		{
			return ValidInt32( value, 0 );
//			value = ValidString( value, "0" );
//			try
//			{
//				return Convert.ToInt32( value );
//			}
//			catch
//			{
//				return 0;
//			}
		}

		public int? ValidInt32NullableEx(string value, int? defaultValue)
		{
			if (string.IsNullOrEmpty(value))
				return defaultValue;
			try
			{
				return Convert.ToInt32(value);
			}
			catch
			{
				return defaultValue;
			}
		}

        /// <summary>
        /// doesn't throw - we should rework all to do the same
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int ValidIntTry(string value, int defaultValue = default(int))
        {
            // TODO: rework all to use TryParse and not to throw errors
            string error;
            return IntTry(value, defaultValue, out error);
        }
        public int IntTry(string value, int defaultValue = default(int))
        {
            // TODO: rework all to use TryParse and not to throw errors
            string error;
            return IntTry(value, defaultValue, out error);
        }
        public int IntTry(string value, int defaultValue, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(value))
            {
                error = "string is empty";
                return defaultValue; // int.MinValue;
            }
            int result;
            if (!int.TryParse(value, out result))
            {
                error = "int parse failed";
                return defaultValue; // int.MinValue;
            }
            return result;
        }

        public int? Int(string value, int? defaultValue = default(int?))
        {
            return IntNullable(value, defaultValue);
        }
        public int? IntNullable(string value, int? defaultValue = default(int?))
        {
            string error;
            return IntNullable(value, defaultValue, out error);
        }
        public int? IntNullable(string value, int? defaultValue, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(value))
            {
                error = "string is empty";
                return defaultValue; // int.MinValue;
            }
            int result;
            if (!int.TryParse(value, out result))
            {
                error = "int parse failed";
                return defaultValue; // int.MinValue;
            }
            return result;
        }

        public int ValidInt32Ex(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return ValidInt32(value, defaultValue);
        }
		public int ValidInt32( string value, int defaultValue )
		{
			value = ValidString( value, defaultValue.ToString() );
			try
			{
				return Convert.ToInt32( value );
			}
			catch
			{
				return defaultValue;
			}
		}
		public int ValidInt32( object value, int defaultValue )
		{
			try
			{
				return (value == null) ?  defaultValue : Convert.ToInt32(value);
			}
			catch
			{
				return defaultValue;
			}
		}
        public long ValidInt64Ex(string value, long defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return ValidInt64(value, defaultValue);
        }
        public long ValidInt64(string value, long defaultValue)
        {
            value = ValidString(value, defaultValue.ToString());
            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return defaultValue;
            }
        }
        public long ValidInt64(object value, long defaultValue)
		{
			try
			{
				return (value == null) ? defaultValue : Convert.ToInt64(value);
			}
			catch
			{
				return defaultValue;
			}
		}
		public float ValidFloat(string value)
        {
            value = ValidString(value, "0f");
            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                return 0f;
            }
        }
        public float ValidFloat(string value, float defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                return 0f;
            }
        }
		public float ValidFloatEx(string value, float defaultValue)
		{
			if (string.IsNullOrEmpty(value))
				return defaultValue;
			try
			{
				return Convert.ToSingle(value);
			}
			catch
			{
				return defaultValue;
			}
		}
		public decimal ValidDecimal(string value)
		{
			value = ValidString( value, "0" );
			try
			{
				return Convert.ToDecimal( value );
			}
			catch
			{
				return 0;
			}
		}
		public decimal ValidDecimal(string value, out string error)
		{
			error = null;
			if (string.IsNullOrEmpty(value))
			{
				error = "string is empty";
				return decimal.MinValue;
			}
			decimal result;
			if (!decimal.TryParse(value, out result))
			{
				error = "decimal parse failed";
				return decimal.MinValue;
			}
			return result;
		}
		public decimal ValidDecimal(string value, decimal defaultValue)
		{
            //if (string.IsNullOrEmpty(value))
            //    return defaultValue;

            value = ValidString( value, defaultValue.ToString() );
			try
			{
				return Convert.ToDecimal( value );
			}
			catch
			{
				return defaultValue;
			}
		}
		public decimal ValidDecimal( object value, decimal defaultValue )
		{
			try
			{
				return (value == null) ?  defaultValue : Convert.ToDecimal(value);
			}
			catch
			{
				return defaultValue;
			}
		}
        public double ValidDoubleEx(string value, double defaultValue = default(double)) // double.MinValue
        {
            string error;
            return ValidDoubleEx(value, defaultValue, out error);
        }
        public double ValidDoubleEx(string value, double defaultValue, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                error = "string is empty";
                return defaultValue; // double.MinValue;
            }
            double result;
            if (!double.TryParse(value, out result))
            {
                error = "double parse failed";
                return defaultValue; // double.MinValue;
            }
            return result;
        }
        public double ValidDouble(object value, double defaultValue)
		{
			try
			{
				return (value == null) ? defaultValue : Convert.ToDouble(value);
			}
			catch
			{
				return defaultValue;
			}
		}
		public float ValidFloat(object value, float defaultValue)
		{
			try
			{
				return (value == null) ? defaultValue : Convert.ToSingle(value);
			}
			catch
			{
				return defaultValue;
			}
		}
		public byte ValidByte(object value, byte defaultValue)
		{
			try
			{
				return (value == null) ? defaultValue : Convert.ToByte(value);
			}
			catch
			{
				return defaultValue;
			}
		}
		public Guid ValidGuid(object value, Guid defaultValue)
		{
			try
			{
				if(value == null) return defaultValue;
				string strguid = ValidString(value, null);
				if( !string.IsNullOrEmpty(strguid) )
					return new Guid(strguid);
				return (value is Guid) ? (Guid)value : Guid.Empty;
			}
			catch
			{
				return defaultValue;
			}
		}

        public TimeSpan ValidTimeSpan(object value, TimeSpan defaultdate)
		{
			if (value == System.DBNull.Value || value == null || !(value is TimeSpan))
				return defaultdate;
			return (TimeSpan)value;
		}
		public TimeSpan ValidTimeSpanParse(string value, TimeSpan defaulttime)
		{
			value = ValidNoneEmptyString(value, defaulttime.ToString());
			try
			{
				TimeSpan result;
				if (TimeSpan.TryParse(value, out result))
					return result;
				return defaulttime;
			}
			catch
			{
				return defaulttime;
			}
		}
		public TimeSpan ValidTimeSpanParseEx(string value, TimeSpan defaulttime)
		{
			if (string.IsNullOrEmpty(value))
				return defaulttime;
			try
			{
				TimeSpan result;
				if (TimeSpan.TryParse(value, out result))
					return result;
				return defaulttime;
			}
			catch
			{
				return defaulttime;
			}
		}
        public DateTime ValidDateTimeTry(string value, DateTime defaultValue, string exactFormat = null)
        {
            string error;
            return ValidDateTimeTry(value, defaultValue, exactFormat, out error);
        }
        public DateTime ValidDateTimeTry(string value, DateTime defaultValue, string exactFormat, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                error = "string is empty";
                return defaultValue;
            }
            DateTime result;
            if (exactFormat != null)
            {
                if (!DateTime.TryParseExact(
                    value, 
                    exactFormat, 
                    System.Globalization.CultureInfo.CurrentCulture, 
                    System.Globalization.DateTimeStyles.None, 
                    out result))
                {
                    error = "datetime parse failed";
                    return defaultValue;
                }
            }
            else
            {
                if (!DateTime.TryParse(value, out result))
                {
                    error = "datetime parse failed";
                    return defaultValue;
                }
            }
            return result;
        }
        public DateTime ValidDateTimeParse(string value, DateTime defaultdate)
		{
			//value = ValidString( value, defaultdate.ToString() );
            value = ValidNoneEmptyString(value, defaultdate.ToString());
            try
			{
				return Convert.ToDateTime( value );
			}
			catch
			{
				return defaultdate;
			}
		}
        public DateTime? ValidDateTimeNullable(string value, DateTime? defaultdate)
        {
            if (string.IsNullOrEmpty(value))
                return defaultdate;
            value = ValidNoneEmptyString(value, defaultdate.ToString());
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return defaultdate;
            }
        }
        public DateTime ValidDateTimeParse(object value, DateTime defaultdate)
		{
			if( value is DateTime )
				return (DateTime)value;
			if( value is string )
				return ValidDateTimeParse( (string)value, defaultdate );
			try
			{
				return (value==System.DBNull.Value || value==null) ?  defaultdate : Convert.ToDateTime(value);
			}
			catch
			{
				return defaultdate;
			}
		}
		public string ValidString( object value, string defaultstr )
		{
			if( value is string )
				return ValidString( (string)value, defaultstr );
			try
			{
				return (value==null) ? defaultstr : Convert.ToString(value);
			}
			catch
			{
				return defaultstr;
			}
		}
		public bool ValidBoolean( string value, bool defaultValue )
		{
			if (string.IsNullOrEmpty(value))
				return defaultValue;
			value = ValidString(value, defaultValue.ToString());
			try
			{
				return Convert.ToBoolean( value );
			}
			catch
			{
				return defaultValue;
			}
		}
		public bool ValidBoolean( object value, bool defaultValue )
		{
			try
			{
				return (value == null) ?  defaultValue : Convert.ToBoolean(value);
			}
			catch
			{
				return defaultValue;
			}
		}



        public bool? Boolean(string value, bool? defaultValue = default(bool?))
        {
            return BooleanNullable(value, defaultValue);
        }
        public bool? BooleanNullable(string value, bool? defaultValue = default(bool?))
        {
            string error;
            return BooleanNullable(value, defaultValue, out error);
        }
        public bool? BooleanNullable(string value, bool? defaultValue, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(value))
            {
                error = "string is empty";
                return defaultValue;
            }
            bool result;
            if (!bool.TryParse(value, out result))
            {
                error = "boolean parse failed";
                return defaultValue;
            }
            return result;
        }

        public bool ValidBooleanTry(string value, bool defaultValue = default(bool))
        {
            string error;
            return ValidBooleanTry(value, defaultValue, out error);
        }
        public bool ValidBooleanTry(string value, bool defaultValue, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                error = "string is empty";
                return defaultValue;
            }
            bool result;
            if (!bool.TryParse(value, out result))
            {
                error = "boolean parse failed";
                return defaultValue;
            }
            return result;
        }

        public object ValidEnum( Type enumType, string value, object defaultValue )
		{
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            //value = ValidNoneEmptyString( value, defaultValue.ToString() );
            ////value = ValidString( value, defaultValue.ToString() );
			try
			{
				return Enum.Parse( enumType, value, true );
			}
			catch
			{
				return defaultValue;
			}
		}
		public object ValidEnum( Type enumType, object value, object defaultValue )
		{
            //if (string.IsNullOrEmpty(value))
            //    return defaultValue;

            try
			{
				string strvalue = ValidString( Convert.ToString(value), defaultValue.ToString() );
				return Enum.Parse( enumType, strvalue, true );
			}
			catch
			{
				return defaultValue;
			}
		}
		public object ValidEnumFromInt32( Type enumType, object value, object defaultValue )
		{
            //if (string.IsNullOrEmpty(value))
            //    return defaultValue;

            try
			{
				int ivalue = this.ValidInt32( value, (int)defaultValue );
				return Enum.ToObject( enumType, ivalue );
			}
			catch
			{
				return defaultValue;
			}
		}
        public string GuidString(Guid guid)
        {
            return guid != Guid.Empty ? guid.ToString() : null;
        }

		public string Valid(object value, string @default) { return ValidString(value, @default); }
		public DateTime Valid(object value, DateTime @default) { return ValidDateTime(value, @default); }
		public TimeSpan Valid(object value, TimeSpan @default) { return ValidTimeSpan(value, @default); }
		public int Valid(object value, int @default) { return ValidInt32(value, @default); }
		public double Valid(object value, double @default) { return ValidDouble(value, @default); }
		public decimal Valid(object value, decimal @default) { return ValidDecimal(value, @default); }
		public byte Valid(object value, byte @default) { return ValidByte(value, @default); }
		public short Valid(object value, short @default) { return (short)ValidInt32(value, (int)@default); }
		public long Valid(object value, long @default) { return ValidInt64(value, @default); }
		public byte[] Valid(object value, byte[] @default) { return (value == null) ? @default : (value as byte[]); }
		public Guid Valid(object value, Guid @default) { return ValidGuid(value, @default); }
		public bool Valid(object value, bool @default) { return ValidBoolean(value, @default); }

		public T Parse<T>(string text)
			//where T : struct
		{
			T value = default(T);
			if (value is string)
				return (T)(object)text;
			else if (value is DateTime)
				return (T)(object)ValidDateTimeParse(text, DateTime.Now);
			else if (value is int)
				return (T)(object)ValidInt32(text);
			else if (value is double)
				return (T)(object)ValidDouble(text, 0);
			else if (value is decimal)
				return (T)(object)ValidDecimal(text);
			else if (value is byte)
				return (T)(object)ValidByte(text, 0);
			else if (value is short)
				return (T)(object)(short)ValidInt32(text, 0);
			else if (value is Guid)
				return (T)(object)ValidGuid(text, default(Guid));
			else if (value is bool)
				return (T)(object)ValidBoolean(text, false);
			else
				return value;
			
		}
	}
}
