using System;
using Microsoft.Win32;

using System.Reflection;
using System.Runtime.InteropServices;

namespace NSDK
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Registry
	{
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly Registry I = new Registry();
		private Registry()
		{
		}

		public bool Exists(RegistryKey RegHive, string RegPath)
		{
			string[] regStrings;
			regStrings = RegPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = RegHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i]);
				if (RegKey[i + 1] == null)
					return false;
				if (i == regStrings.Length - 1)
					return true;
			}
			return false;
		}

		public void Write(RegistryKey RegHive, string RegPath, string KeyName, string KeyValue)
		{
			string[] regStrings;
   			regStrings = RegPath.Split('\\'); 
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1]; 
			RegKey[0] = RegHive; 
  
			for( int i = 0; i < regStrings.Length; i++ )
			{ 
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i], true);
				if (RegKey[i + 1] == null)  
					RegKey[i + 1] = RegKey[i].CreateSubKey(regStrings[i]);
			} 
			
			try
			{
				RegKey[regStrings.Length].SetValue(KeyName, KeyValue);     
			}
			catch (System.NullReferenceException)
			{
				throw(new Exception("Null Reference"));
			}
			catch (System.UnauthorizedAccessException)
			{
    			throw(new Exception("Unauthorized Access"));
			}
		}

		public string ReadValid(RegistryKey regHive, string regPath, string keyName, string defaultValue)
		{
			return ReadInternal(regHive, regPath, keyName, defaultValue, true);
		}
		public string Read(RegistryKey regHive, string regPath, string keyName, string defaultValue)
		{
			return ReadInternal(regHive, regPath, keyName, defaultValue, false);
		}
		
		public object ReadValid(RegistryKey regHive, string regPath, string keyName, object defaultValue)
		{
			return ReadInternal(regHive, regPath, keyName, defaultValue, true);
		}
		public object Read(RegistryKey regHive, string regPath, string keyName, object defaultValue)
		{
			return ReadInternal(regHive, regPath, keyName, defaultValue, false);
		}

		string ReadInternal(RegistryKey RegHive, string RegPath, string KeyName, string defaultValue, bool makevalid)
		{
			string[] regStrings;
			string result = "";

			regStrings = RegPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = RegHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i]);
				if (RegKey[i + 1] == null)
					return makevalid ? defaultValue : null;
				if (i == regStrings.Length - 1)
					result = (string)RegKey[i + 1].GetValue(KeyName, defaultValue);
			}
			return result;
		}
		object ReadInternal(RegistryKey regHive, string regPath, string keyName, object defaultValue, bool makevalid)
		{
			string[] regStrings;
			object result = "";

			regStrings = regPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = regHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i]);
				if (RegKey[i + 1] == null)
					return makevalid ? defaultValue : null;
				if (i == regStrings.Length - 1)
					result = RegKey[i + 1].GetValue(keyName, defaultValue);
			}
			return result;
		}

		public void Write(RegistryKey RegHive, string RegPath, string KeyName, object KeyValue, RegistryValueKind kind)
		{
			string[] regStrings;
			regStrings = RegPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = RegHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i], true);
				if (RegKey[i + 1] == null)
					RegKey[i + 1] = RegKey[i].CreateSubKey(regStrings[i]);
			}

			try
			{
				RegKey[regStrings.Length].SetValue(KeyName, KeyValue, kind);
			}
			catch (System.NullReferenceException)
			{
				throw (new Exception("Null Reference"));
			}
			catch (System.UnauthorizedAccessException)
			{
				throw (new Exception("Unauthorized Access"));
			}
		}

		public void Delete(RegistryKey RegHive, string RegPath, string KeyName)
		{
			string[] regStrings;
			regStrings = RegPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = RegHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i], true);
				if (RegKey[i + 1] == null)
					return;
					//RegKey[i + 1] = RegKey[i].CreateSubKey(regStrings[i]);
			}

			try
			{
				RegKey[regStrings.Length].DeleteValue(KeyName, false);
			}
			catch (System.NullReferenceException)
			{
				throw (new Exception("Null Reference"));
			}
			catch (System.UnauthorizedAccessException)
			{
				throw (new Exception("Unauthorized Access"));
			}
		}

		public void DeleteKey(RegistryKey RegHive, string RegPath, string KeyName, bool dothrow)
		{
			string[] regStrings;
			regStrings = RegPath.Split('\\');
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1];
			RegKey[0] = RegHive;

			for (int i = 0; i < regStrings.Length; i++)
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i], true);
				if (RegKey[i + 1] == null)
					return;
				//RegKey[i + 1] = RegKey[i].CreateSubKey(regStrings[i]);
			}

			try
			{
				RegKey[regStrings.Length].DeleteSubKeyTree(KeyName);
			}
			catch (NullReferenceException)
			{
				throw (new Exception("Null Reference"));
			}
			catch (UnauthorizedAccessException)
			{
				throw (new Exception("Unauthorized Access"));
			}
			catch (Exception e)
			{
				if (dothrow)
					throw e;
			}
		}

		public static IntPtr GetRegistryHandle(RegistryKey registryKey)
		{
			Type type = registryKey.GetType();
			FieldInfo fieldInfo = type.GetField("hkey", BindingFlags.Instance |
			BindingFlags.NonPublic);
			return (IntPtr)fieldInfo.GetValue(registryKey);
		}

		public static RegistryKey GetKeyFromHandle(IntPtr hkey) 
		{
			try
			{
				RegistryKey testobj = Microsoft.Win32.Registry.CurrentUser;
				testobj = testobj.OpenSubKey(@"Software\Microsoft\Internet Explorer\InternetRegistry\REGISTRY\USER", true);
				if (testobj == null)
				{
					Log.Debug($"GetKeyFromHandle:...testobj is null...{ testobj }");
					return null;
				}
				Type regkeyType = testobj.GetType();

				FieldInfo fieldInfo = regkeyType.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo == null)
				{
                    Log.Debug($"GetKeyFromHandle:...hkey field is null...{ fieldInfo }");
					return null;
				}

				ConstructorInfo ci = fieldInfo.FieldType.GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { typeof(IntPtr), typeof(bool) },
					null);
				object objSafeHandle = ci.Invoke(new object[] { hkey, false });

				ci = regkeyType.GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { fieldInfo.FieldType, typeof(bool) },
					null);
				object objKey = ci.Invoke(new object[] { objSafeHandle, true });
				//object objKey = ci.Invoke(new object[] { hkey, false });
				return objKey as RegistryKey;

				//object objSafeHandle = Marshal.PtrToStructure(hkey, fieldInfo.FieldType); 
				//object objKey = Activator.CreateInstance(regkeyType, objSafeHandle, true);
				////regkeyType.TypeInitializer.
				//return objKey as RegistryKey;



				////fieldInfo.SetValue(testobj, hkey);

				//object objSafeHandle = fieldInfo.GetValue(testobj);
				//FieldInfo fieldInfoHandle = fieldInfo.FieldType.GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic);

				//SafeLog.I.Debug("GetKeyFromHandle:...hkey handle value is ...{0}", (fieldInfoHandle.GetValue(fieldInfo.GetValue(testobj))).ToString());
				//fieldInfoHandle.SetValue(objSafeHandle, hkey);
				//SafeLog.I.Debug("GetKeyFromHandle:...hkey handle value after change is ...{0}", (fieldInfoHandle.GetValue(fieldInfo.GetValue(testobj))).ToString());

				//return testobj;

				RegistryKey key = (RegistryKey)regkeyType.InvokeMember("GetBaseKey",
					BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
					null, null,
					new object[] { hkey });
				return key;
			}
			catch (Exception e)
			{
                Log.Error($"error...", e);
				return null;
			}
		}

		// this is a quick/dirty fix for .NET 4.0 what it seems (evne though this isn't 4.0)
		public static RegistryKey GetKeyFromHandle1(IntPtr hkey)
		{
			try
			{
				RegistryKey testobj = Microsoft.Win32.Registry.CurrentUser;
				testobj = testobj.OpenSubKey(@"Software\Microsoft\Internet Explorer\InternetRegistry\REGISTRY\USER", true);
				if (testobj == null)
				{
					//SafeLog.I.Debug("GetKeyFromHandle:...testobj is null...{0}", testobj);
					return null;
				}
				Type regkeyType = testobj.GetType();

				FieldInfo fieldInfo = regkeyType.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo == null)
				{
					//SafeLog.I.Debug("GetKeyFromHandle:...hkey field is null...{0}", fieldInfo);
					return null;
				}

				//System.Diagnostics.Debugger.Break();
				ConstructorInfo ci = fieldInfo.FieldType.GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					//BindingFlags.Instance,
					null,
					new Type[] { typeof(IntPtr), typeof(bool) },
					null);
				if (ci == null)
				{
					ci = fieldInfo.FieldType.GetConstructor(
						BindingFlags.Public | BindingFlags.Instance,
						//BindingFlags.Instance,
						null,
						new Type[] { typeof(IntPtr), typeof(bool) },
						null);
					if (ci == null)
					{
						ci = fieldInfo.FieldType.GetConstructors(BindingFlags.Instance)[0];
					}
					//System.Diagnostics.Debugger.Break();
					////Microsoft.Win32.SafeHandles.SafeRegistryHandle
					//foreach (var c in fieldInfo.FieldType.GetConstructors(BindingFlags.Instance)) // BindingFlags.NonPublic | 
					//{
					//  foreach(var p in c.GetParameters())
					//    SafeLog.I.Debug("{0}, {1}", c.Name, p.ParameterType);
					//}
				}

				object objSafeHandle = ci.Invoke(new object[] { hkey, false });

				ci = regkeyType.GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[] { fieldInfo.FieldType, typeof(bool) },
					null);
				if (ci == null)
				{
 					//RegistryKey.FromHandle
				}

				object objKey = ci.Invoke(new object[] { objSafeHandle, true });
				//object objKey = ci.Invoke(new object[] { hkey, false });
				return objKey as RegistryKey;
			}
			catch (Exception e)
			{
                Log.Error($"error...", e);
                //SafeLog.I.Error(e);
				return null;
			}
		}

	}
}




