using BlueMaria.Services;
using BlueMaria.Services.Impl;
using BlueMaria.StaticFunction;
using BlueMaria.View;
using BlueMaria.ViewModels;
using BlueMariaLocalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlueMaria
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int SW_NORMAL = 1;
        private Mutex myMutex;
        private View.MainWindow _main;
        public static bool StartUp;
        MainViewModel MainViewModel { get; set; }
        RecordingService RecordingService { get; set; }

        private System.Timers.Timer refreshTimer = null;
        private const int RefreshInterval = 24*60*60*1000;
        private int numberOfDays;

        protected override void OnStartup(StartupEventArgs e)
        {
            this.numberOfDays = 0;
            this.refreshTimer = new System.Timers.Timer();
            this.refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.RefreshDomEveryDay);
            this.refreshTimer.Interval = RefreshInterval;
            this.refreshTimer.Enabled = true;

            if (!this.CheckandDownload())
            {
                Shutdown(1);
            }
            
            SetAppData();     
            bool aIsNewInstance = false;
            base.OnStartup(e);
            myMutex = new Mutex(true, "BlueMaria", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                App.Current.Shutdown();
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        IntPtr pointer = FindWindow(null, "Blue-Maria");
                        SetForegroundWindow(pointer);
                        ShowWindow(pointer, SW_NORMAL);
                        break;
                    }
                }
            }
            else
            {
                ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                for (int i = 0; i != e.Args.Length; ++i)
                {
                    if (e.Args[i] == "--Startup")
                    {
                        LocalSettings.StartUp = true;
                        break;
                    }
                }
                Splashscreen splsh = new Splashscreen();
                if (!LocalSettings.StartUp)
                {
                    splsh.Show();
                }
                var bw = new BackgroundWorker();
                bw.DoWork += (s5, e5) =>
                  {
                      // use this to start the log4net (or in AssemblyInfo.cs, this seems safer)
                      log4net.Config.XmlConfigurator.Configure();

                      Log.Debug($"{ e.ToString()}: testing..."); //, e);

                      this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                      TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                      // initialize all our 'service' classes
                      // use IoC but an overkill for just this

                      //Image previousImage = null;

                      var screenService = new ScreenService(
                          isAvailable =>
                          {
                              this.MainViewModel.IsNetworkAvailable = isAvailable;
                              LocalSettings.Netcheck = isAvailable;
                              if (LocalSettings.LoginCheck&& this.MainViewModel.CanRecord)
                              {
                                  this.MainViewModel.IsLoggedIn = isAvailable;
                              }
                              if (isAvailable)
                              {
                                  LocalSettings.ChangeIcon("Grey");
                                  Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { LocalSettings.StopRecordApi?.Invoke(this, EventArgs.Empty); }));
                              }
                              if(!isAvailable)
                              {
                                  LocalSettings.ChangeIcon("Red");
                                  //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LocalSettings.Kill?.Invoke(this, EventArgs.Empty)));
                              }

                          },
                          canRecord =>
                          {
                              this.MainViewModel.CanRecord = canRecord;
                              //LocalSettings.Netcheck = canRecord;
                              if (LocalSettings.LoginCheck && this.MainViewModel.IsNetworkAvailable)
                              {
                                  this.MainViewModel.IsLoggedIn = canRecord;
                              }
                             

                              if (canRecord)
                              {
                                  LocalSettings.ChangeIcon("Grey");
                                  //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { LocalSettings.StopRecordApi?.Invoke(this, EventArgs.Empty); }));
                              }
                              if(!canRecord)
                              {
                                  LocalSettings.ChangeIcon("Red");
                                  //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LocalSettings.Kill?.Invoke(this, EventArgs.Empty)));
                              }
                          });

                      var networkService = new NetworkService(screenService);
                      this.RecordingService = new RecordingService(screenService);
                      var webAPIService = new WebAPIService();
                      this.MainViewModel = new MainViewModel(
                      screenService, networkService, this.RecordingService, webAPIService);


                  };
                bw.RunWorkerCompleted += (s6, e6) =>
                {                    
                      _main = new View.MainWindow();
                      var _mainpage = new View.MainPage(_main);
                      var _shrinkpage = new View.ShrinkedPage(_main);
                      var _settingsviewmodel = new ViewModels.SettingPageViewModel();
                      _main.DataContext = this.MainViewModel;                    
                      _mainpage.DataContext = this.MainViewModel;
                      _shrinkpage.DataContext = this.MainViewModel;
                      Setlanguage();
                      _main.Loaded += async (s3, ev) =>
                      {
                          await this.MainViewModel.OnLoaded();
                      };
                      LocalSettings.LoadMainPage += (s1, ev) =>
                      {
                          LocalSettings.IsMainpageLoaded = true;
                          LocalSettings.WindowShrinked = false;
                          _main.MainFrame.Navigate(_mainpage);
                          
                      };
                    LocalSettings.LoadMainPageforshrink  +=  (s1, ev) =>
                    {
                        var index = this.MainViewModel.SelectedLanguageIndex;
                        this.MainViewModel.SelectedLanguageIndex = this.MainViewModel.SelectedLanguageIndex+1;
                        this.MainViewModel.SelectedLanguageIndex = index;
                        _main.DataContext = this.MainViewModel;                                                
                        _main.Height = 600;
                        _main.Width = 370;

                        //LocalSettings.IsMainpageLoaded = true;
                        LocalSettings.WindowShrinked = false;
                        _main.MainFrame.Navigate(_mainpage);

                    };
                    LocalSettings.LoadShrikedPage += (s2, ev2) =>
                      {
                          LocalSettings.WindowShrinked = true;
                          LocalSettings.IsMainpageLoaded = false;
                          _main.MainFrame.Navigate(_shrinkpage);
                      };
                      LocalSettings.LoadSettings += (s4, ev4) =>
                      {
                          LocalSettings.WindowShrinked = false;
                          LocalSettings.IsMainpageLoaded = false;
                          _settingsviewmodel.Top = MainViewModel.Top;
                          _settingsviewmodel.Left = MainViewModel.Left;
                          _main.DataContext = _settingsviewmodel;
                         if(_main.Height != 600 & _main.Width != 600) { 
                          _main.Height = 600;
                          _main.Width = 600;
                          }
                          var _settingspage = new View.SettingPage(_main);
                          _main.MainFrame.Navigate(_settingspage);
                          _settingspage = null;
                      };
                      LocalSettings.LoadDataContext += (s8, e8) =>
                    {
                        this.MainViewModel.Top = _settingsviewmodel.Top;
                        MainViewModel.Left = _settingsviewmodel.Left;

                        _main.DataContext = this.MainViewModel;

                        _main.Height = 600;
                        _main.Width = 370;
                    };
                      LocalSettings.LoadShrikedPageDatacontext += (s7, ev7) =>
                    {
                        if (LocalSettings.WindowShrinked)
                        {
                            _main.Height = 113.422818791946308724832214765101;
                            _main.Width = 75.0671140939597;
                            _main.DataContext = this.MainViewModel;
                        }
                    };
                    _main.Closed += MainWindow_Closed;
                      _main.Closing += MainWindow_Closing;
                      if (this.MainViewModel.IsMinimized)
                      {
                        LocalSettings.WindowShrinked = true;
                        LocalSettings.IsMainpageLoaded = false;
                        LocalSettings.PasswrdLogout?.Invoke(this, EventArgs.Empty);
                        _main.MainFrame.Navigate(_shrinkpage);
                      }
                      else
                      {
                          var r =this.MainViewModel.IsLoggedIn;
                        LocalSettings.PasswrdLogout?.Invoke(this, EventArgs.Empty);
                        _main.MainFrame.Navigate(_mainpage);
                      }

                      this.MainViewModel.Started += (s, ev) =>
                      {
                          if (this.MainViewModel.IsListening && !this.MainViewModel.IsMinimized)
                          {
                              this.Dispatcher.BeginInvoke(
                                  (Action)(() =>
                                  {
                                      LocalSettings.WindowShrinked = true;
                                      LocalSettings.IsMainpageLoaded = false;
                                      _main.MainFrame.Navigate(_shrinkpage);
                                      this.MainViewModel.IsMinimized = !this.MainViewModel.IsMinimized;
                                  }));
                          }
                      };

                      if (LocalSettings.StartUp)
                      {
                          _main.Visibility = Visibility.Hidden;
                        //_main.ShowInTaskbar = false;
                        LocalSettings.StartUp = false;
                      }
                      else
                      {
                          splsh.Close();
                      }
                      LocalSettings.Showwindow += (s9, e9) =>
                      {
                          var index = this.MainViewModel.SelectedLanguageIndex;
                          this.MainViewModel.SelectedLanguageIndex = this.MainViewModel.SelectedLanguageIndex + 1;
                          this.MainViewModel.SelectedLanguageIndex = index;
                          _main.Visibility = Visibility.Visible;
                          //_main.ShowInTaskbar = true;
                      };
                      splsh = null;
                };
                bw.RunWorkerAsync();
            }          
        }
        /// <summary>
        /// A timer which runs per day to check if the update is required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshDomEveryDay(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.numberOfDays++;
            if (this.numberOfDays % 15 == 0)
            {
                if (!this.CheckandDownload())
                {
                    Shutdown();
                }
            }
        }

        /// <summary>
        /// Get the product version from the registry and check with the latest on the server using the API,
        /// if the local version is old then give chance for the user to install the latest one.
        /// </summary>
        /// <returns></returns>
        private bool CheckandDownload()
        {

            //Get the product version from the registry
            string productVersion = RegistryService.ReadRegistryEntry("ProductVersion");
            if (string.IsNullOrEmpty(productVersion))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show($"Make sure you have installed BlueMaria, Contact administrator for help",
                                "Blue-Maria",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }
            if (!CheckUpdate(productVersion))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show($"You are running an old version of this software.Please install the new version now.\n\nOK - Continue with update process\nCancel - Close application\n\nThe application will not start anymore until you have updated to the latest version.Thank you.",
                                "Blue-Maria",
                                MessageBoxButton.OKCancel,
                                MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    result = System.Windows.MessageBox.Show($"ATTENTION: If you have changed any of the *.def files in the keytables folder of this application, we strongly recommend, that you do a backup of these files, on your own, prior to continue.\n\nOK - Continue installation process\nCancel - Cancel installation process to manually backup the files\n\nIf you \"Cancel\" you can restart the update process again by closing and re - starting the application.",
                                "Blue-Maria",
                                MessageBoxButton.OKCancel,
                                MessageBoxImage.Warning);
                    if (result == MessageBoxResult.OK)
                    {

                        this.DownloadInstaller();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Download the installer from the blue-maria site and register the callback when the download is completed.
        /// </summary>
        private void DownloadInstaller()
        {
            Uri uri = new Uri("https://blue-maria.com/download/Blue-Maria.exe");
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp/Blue-Maria.exe");

            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                WebClient wc = new WebClient();
                wc.DownloadFileAsync(uri, filename);

                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                Splashscreen splsh = new Splashscreen();
                splsh.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        /// <summary>
        /// when the download is complete start the installer and then exit the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp/Blue-Maria.exe");
                Process.Start(filename);
                Shutdown(1);
            }
            else
            {
                MessageBox.Show("Unable to download exe, please check your connection", "Download failed!");
            }
        }

        /// <summary>
        /// Check if the installed version of the tool is latest by making an API call to the server
        /// </summary>
        /// <param name="productVersion">Installed product version</param>
        /// <returns>True if update required else False</returns>
        private bool CheckUpdate(string productVersion)
        {
            var page = $"https://blue-maria.com/bmapi-nov09th2018-v1/CheckUpdate.php?";

            if (!string.IsNullOrEmpty(productVersion))
            {
                page = page + $"product_ver={productVersion}";
            }

            try
            {
                // Create a request for the URL.
                WebRequest request = WebRequest.Create(page);

                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;

                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // check status code
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else if (statusCode == HttpStatusCode.Gone)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                // Only for exception 410 - which means the resource is gone we return false from this method
                // which indicates that there is a new update and that needs to be installed.
                // for any other exception return true as it would be handled later in the code.
                if (e.Message.Contains("410"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public void Setlanguage()
        {
            LocalSettings.CurrentLaguage = BlueMaria.Properties.Settings.Default.ApplicationLanguage;
            LocalSettings.AutoLaunch = BlueMaria.Properties.Settings.Default.AutoLaunch;
            LocalizationManager.UICulture = CultureInfo.GetCultureInfo(LocalSettings.CurrentLaguage);
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                this.RecordingService.Stop(); // should use Dispose/IDisposable instead but it's the same
                //this.MainViewModel.OnClosed();
                this.Shutdown();
            }
            catch (Exception)
            {
                //Log.I.Error(ev1);
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                this.MainViewModel.OnClosing();            
               // Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LocalSettings.Kill?.Invoke(this, EventArgs.Empty)));
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { LocalSettings.StopRecordApi?.Invoke(this, EventArgs.Empty); }));
                LocalSettings.ExitApplication();
            }
            catch (Exception)
            {
                //Log.I.Error(ev1);
            }
        }

        private void SetAppData()
        {

            try
            {

                BlueMaria.Properties.Settings.Default.AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\BlueMaria";
                bool exists = Directory.Exists(BlueMaria.Properties.Settings.Default.AppData);
                if (!exists)
                {
                    Directory.CreateDirectory(BlueMaria.Properties.Settings.Default.AppData);
                    string SourcePath = Directory.GetCurrentDirectory() + @"\BlueMaria.cfg";
                    string DestinationPath = BlueMaria.Properties.Settings.Default.AppData + @"\BlueMaria.cfg";
                    File.Copy(SourcePath, DestinationPath, true);
                }

                BlueMaria.Properties.Settings.Default.Save();

            }
            catch (Exception ex)
            {
                Log.Error("Setting Appdata error...", ex);
            }

        }

        #region Events

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error($"{ e.ToString()}: { e.ExceptionObject?.ToString()}"); //, e);
            //Debugger.Launch();
            if (e.IsTerminating) return;
#if SHOW_GLOBAL_EXCEPTIONS_IN_MESSAGE_BOX
            MessageBox.Show(e.ExceptionObject.ToString());
#endif
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception.Message);
#if SHOW_GLOBAL_EXCEPTIONS_IN_MESSAGE_BOX
            MessageBox.Show("Task unobserved exception: " + e.Exception.Message.ToString());
#endif
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception);
            if (e.Exception.InnerException != null)
            {
                Log.Error(e.Exception.InnerException);
#if SHOW_GLOBAL_EXCEPTIONS_IN_MESSAGE_BOX
                MessageBox.Show("UNHANDLED EXCEPTION: " +
                    Environment.NewLine + Environment.NewLine +
                    e.Exception.InnerException.GetType().Name +
                    Environment.NewLine + Environment.NewLine +
                    e.Exception.InnerException.Message);

                if (e.Exception.InnerException.InnerException != null)
                    MessageBox.Show("UNHANDLED EXCEPTION: " +
                        Environment.NewLine + Environment.NewLine +
                        e.Exception.InnerException.InnerException.GetType().Name +
                        Environment.NewLine + Environment.NewLine +
                        e.Exception.InnerException.InnerException.Message);
#endif
            }
#if SHOW_GLOBAL_EXCEPTIONS_IN_MESSAGE_BOX
            MessageBox.Show("UNHANDLED EXCEPTION: " +
                Environment.NewLine + Environment.NewLine +
                e.Exception.GetType().Name +
                Environment.NewLine + Environment.NewLine +
                e.Exception.Message);
#endif
            e.Handled = true;
            //this.Shutdown();
        }

        #endregion

    }
}
