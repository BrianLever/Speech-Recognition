using BlueMaria.Commands;
using BlueMaria.StaticFunction;
using BlueMariaLocalization;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BlueMariaLocalization.Properties;
using System.Windows.Threading;
using System.Windows;

namespace BlueMaria.ViewModels
{
    public class SettingPageViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
        public DelegateCommand SaveCommand { get; }
        public List<Languages> SupportedLanguages
        {
            get
            {
                List<Languages> lang = new List<Languages>();
                for (int i = 0; i < LocalSettings.SupportedLanguages.Count; i++)
                {
                    Languages obj = new Languages();
                    obj.Language = LocalSettings.Laguages[i];
                    obj.LanguageCode = LocalSettings.SupportedLanguages[i];
                    lang.Add(obj);
                    obj = null;
                }
               
                return lang;
            }
        }
        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        public string Language
        {
            get { return LocalSettings.CurrentLaguage; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    LocalSettings.CurrentLaguage = "";
                }
                else if (LocalSettings.SupportedLanguages.Contains(value))
                {
                    LocalSettings.CurrentLaguage = value;
                }
            }
        }
        public bool IsAutolaunch
        {
            get
            {
                return LocalSettings.AutoLaunch;
            }
            set
            {
                if (LocalSettings.AutoLaunch == value) return;
                LocalSettings.AutoLaunch = value;
                LocalSettings.AutoLaunch = value;
                this.OnPropertyChanged(nameof(IsAutolaunch));
            }
        }
        private int _left = 100;
        public int Left
        {
            get { return this._left; }
            set
            {
                if (this._left == value) return;
                this._left = value;
                this.OnPropertyChanged(nameof(Left));
            }
        }
        private int _top = 100;
        public int Top
        {
            get { return this._top; }
            set
            {
                if (this._top == value) return;
                this._top = value;
                this.OnPropertyChanged(nameof(Top));
            }
        }
        private int _width = 600;
        public int Width
        {
            get { return this._width; }
            set
            {
                if (this._width == value) return;
                this._width = value;

                this.OnPropertyChanged(nameof(Width));
            }
        }

        private int _height = 600;
        public int Height
        {
            get { return this._height; }
            set
            {
                if (this._height == value) return;
                this._height = value;
                this.OnPropertyChanged(nameof(Height));
            }
        }

        private string _error = "Collapsed";
        public string Error
        {
            get { return this._error; }
            set
            {
                if (this._error == value) return;
                this._error = value;
                this.OnPropertyChanged(nameof(Error));
               
            }
        }

        public SettingPageViewModel()
        {
            SaveCommand = new DelegateCommand(Save);            
        }
        public void Save()
        {
            try
            {
               

                    //  LocalSettings.ChangeIcon("HalfGreen");
                var AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Windows\Start Menu\Programs\Startup";
                if (LocalSettings.CurrentLaguage != "" && LocalSettings.CurrentLaguage != BlueMaria.Properties.Settings.Default.ApplicationLanguage)
                {
                    BlueMaria.Properties.Settings.Default.ApplicationLanguage = LocalSettings.CurrentLaguage;
                    LocalizationManager.UICulture = CultureInfo.GetCultureInfo(LocalSettings.CurrentLaguage);
                    LocalSettings.ChangeIcon(LocalSettings.currentfavicon);

                }
                if (LocalSettings.AutoLaunch && LocalSettings.AutoLaunch != BlueMaria.Properties.Settings.Default.AutoLaunch)
                {
                    BlueMaria.Properties.Settings.Default.AutoLaunch = LocalSettings.AutoLaunch;
                    var SourcePath = new System.IO.FileInfo(System.IO.Directory.GetCurrentDirectory());
                    {
                        WshShell shell = new WshShell();
                        string shortcutAddress = AppdataPath + @"\BlueMaria.lnk";
                        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                        shortcut.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
                        shortcut.TargetPath = System.IO.Directory.GetCurrentDirectory() + @"\BlueMariaStartUp.exe";
                        shortcut.IconLocation = System.IO.Directory.GetCurrentDirectory()+ @"\Favicon\Green.ico";
                        shortcut.Save();
                        shell = null;
                        shortcutAddress = null;
                        shortcut = null;
                        SourcePath = null;
                    }
                }
                else if (LocalSettings.AutoLaunch == false && LocalSettings.AutoLaunch != BlueMaria.Properties.Settings.Default.AutoLaunch)
                {
                    BlueMaria.Properties.Settings.Default.AutoLaunch = LocalSettings.AutoLaunch;
                    var Path = AppdataPath + @"\BlueMaria.lnk";
                    if (System.IO.File.Exists(Path))
                    {
                        System.IO.File.Delete(Path);
                    }
                }
                var _filesList = new List<string>();
                // ResourceManager translaa = new ResourceManager(LocalSettings.CurrentLaguage, typeof(SettingPageViewModel).Assembly);          
                _filesList.Add(Resources.SettingsSaved);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty)));
                BlueMaria.Properties.Settings.Default.Save();
            }
            catch
            {
                Error = "Visible";
            }
        }
    }
   public class Languages
    {
        public string Language { get; set; }
        public string LanguageCode { get; set; }
    }

}
