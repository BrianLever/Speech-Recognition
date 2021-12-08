using Microsoft.Win32;
using System;

namespace BlueMaria.Services.Impl
{
    public class RegistryService
    {
        private const string BLUEMARIAREGKEY = "Software\\Blue-maria";
        public static string ReadRegistryEntry(string regKey)
        {
            var regValue = string.Empty;
            try
            {
                RegistryKey rkCurrentUser = Registry.CurrentUser;
                // Obtain the test key (read-only) and display it.
                RegistryKey key = rkCurrentUser.OpenSubKey(BLUEMARIAREGKEY);
                if (key != null)
                {
                    Object o = key.GetValue(regKey);
                    if (o != null)
                    {
                        regValue = o as String;
                    }
                    key.Close();
                }
                rkCurrentUser.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return regValue;
        }

        public static string ReadRegistryEntry(string regOpenKey, string regKey)
        {
            var regValue = string.Empty;
            try
            {
                RegistryKey rkCurrentUser = Registry.CurrentUser;
                // Obtain the test key (read-only) and display it.
                RegistryKey key = rkCurrentUser.OpenSubKey(regOpenKey);
                if (key != null)
                {
                    Object o = key.GetValue(regKey);
                    if (o != null)
                    {
                        regValue = o as String;
                    }
                    key.Close();
                }
                rkCurrentUser.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return regValue;
        }

        public static void SetRegistryEntry(string name, string data)
        {
            try
            {
                RegistryKey rkCurrentUser = Registry.CurrentUser;
                // Obtain the test key (read-only) and display it.
                RegistryKey key = rkCurrentUser.OpenSubKey(BLUEMARIAREGKEY, true);
                if (key == null)
                {
                    key = rkCurrentUser.CreateSubKey(BLUEMARIAREGKEY);
                }

                key.SetValue(name, data);
                key.Close();
                rkCurrentUser.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //"Software\\AddonSuite\\Templates
        public static void SetRegistryEntry(string regkey, string name, string data)
        {
            try
            {
                RegistryKey rkCurrentUser = Registry.CurrentUser;
                // Obtain the test key (read-only) and display it.
                RegistryKey key = rkCurrentUser.OpenSubKey(regkey, true);
                if (key == null)
                {
                    key = rkCurrentUser.CreateSubKey(regkey);
                }

                key.SetValue(name, data);
                key.Close();
                rkCurrentUser.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void RemoveRegistryEntry(string keyPath, string keyName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue(keyName);
                }
            }
        }
    }
}
