// -----------------------------------------------------------------------
// <copyright file="ConfigHandler.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace BlueMaria.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    using NSDK;
    using MSW32 = Microsoft.Win32;
    using System.IO;
    using BlueMaria.Utilities;

    public static class AppConfigDefault
    {
        private static bool _useDefaultAppEvents = false;
        private static bool _useDefaultLogging = true;
        private static bool _useDefaultGUIService = false;
        private static bool _useDefaultConfigHandler = false;
        private static bool _registerDefault = true;
        private static bool _autoWatch = true;
        private static bool _isEditMode = true;
        private static bool? _visibility = null;

        #region Private Methods

        static string GetSafeSettings(string name)
        {
            return null;
            //return ConfigurationManager.AppSettings[name];
        }

        public static MSW32.RegistryKey WritableHKCU
        {
            get
            {
                return Microsoft.Win32.Registry.CurrentUser;
            }
        }
        public static string RegPath { get { return RegCompanyRoot + @"\BMWPFApp"; } }
        public static string RegCompanyRoot { get { return RegCompanyRootBase; } }
        public static string RegCompanyRootBase { get { return @"Software\BlueMaria"; } }

        static bool GetRegistryBool(string name, bool bydefault)
        {
            if (bydefault)
                return Ensure.Is.ValidInt32Ex(Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, name, "1"), 1) == 1;
            else
                return Ensure.Is.ValidInt32Ex(Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, name, "0"), 0) == 1;
        }
        static bool GetRegistryBool(string regkey, string configkey, bool defaultValue = false)
        {
            bool bydefault = Ensure.Is.ValidBoolean(GetSafeSettings(configkey), defaultValue);
            if (bydefault)
                return Ensure.Is.ValidInt32Ex(Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, regkey, "1"), 1) == 1;
            else
                return Ensure.Is.ValidInt32Ex(Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, regkey, "0"), 0) == 1;
        }
        static bool? GetRegistryBoolNullable(string regkey, bool? defaultValue = null)
        {
            var value = Ensure.Is.ValidInt32NullableEx(Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, regkey, (string)null), (int?)null);
            if (value == 1) return true;
            if (value == 0) return false;
            return defaultValue;
        }
        static void SetRegistryBool(string regkey, bool value)
        {
            Registry.I.Write(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, regkey, value ? "1" : "0");
        }
        #endregion

        public static bool SecurityAllowSavingPassword
        {
            get
            {
                // check default profile or something
                return Ensure.Is.ValidInt32Ex(Registry.I.Read(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "SecurityAllowSavingPassword", "0"), 0) == 1;
            }
        }

        public static bool RememberMeByDefault { get { return true; } }
        public static bool RememberMe { get { return RememberMeByDefault && GetRegistryBool("RememberMe", "RememberMe", false); } set { SetRegistryBool("RememberMe", value); } }

        public static string UserName
        {
            get
            {
                return Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "UserName", (string)null);
            }
            set
            {
                Registry.I.Write(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "UserName", value ?? "");
            }
        }

        public static string Password
        {
            get
            {
                var password = Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "Password", (string)null);
                return password != null ? Encryptor.I.Decrypt(password) : null;
                //return password; // password != null ? Encryptor.I.Decrypt(password) : null;
                //return Registry.I.ReadValid(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "Password", (string)null);
            }
            set
            {
                var passtext = Encryptor.I.Encrypt(value);
                //var passtext = value; // Encryptor.I.Encrypt(value);
                Registry.I.Write(AppConfigDefault.WritableHKCU, AppConfigDefault.RegPath, "Password", passtext ?? "");
            }
        }
    }
}
