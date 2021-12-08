using BlueMaria.Commands;
using BlueMaria.Services;
using BlueMaria.StaticFunction;
using BlueMaria.Utilities;
using BlueMariaLocalization.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace BlueMaria.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Consts, Static

        static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";
        // The WS_EX_NOACTIVATE value for dwExStyle prevents foreground
        // activation by the system.
        private const long WS_EX_NOACTIVATE = 0x08000000L;

        // WM_NCMOUSEMOVE Message is posted to a window when the cursor is 
        // moved within the nonclient area of the window. 
        private const int WM_NCMOUSEMOVE = 0x00A0;

        // WM_NCLBUTTONDOWN Message is posted when the user presses the left
        // mouse button while the cursor is within the nonclient area of a window. 
        private const int WM_NCLBUTTONDOWN = 0x00A1;

        private const int WM_NCHITTEST = 0x84;
        //		private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        private const string CHAR_BEFORE_CAP = ".:?!";
        private const string CHAR_NO_SPACE = ".,?!";

        private static TimeSpan _pingInterval = TimeSpan.FromMinutes(1);

        // DRAGAN: rollover is no longer used as such (previously 2 processed were swapped, overlapping briefly)
        // but we still have use of that interval. Each 45 seconds we're clearing the 'log' in the window - and if after 
        // another 45 seconds that log is empty we're automatically stopping the mic, closing recording (and killing the console exe)
        // we need to do that to minimize Google API usage, so after a period of inactivity we're closing. That was previously 
        // native and part of how the processes were swapped. Now, I'm just keeping this 'sleep' function, where rollover is used
        // as inactivity 'period' (inactivity is in fact greater than that, as we first have to finish previous period then
        // let one more go w/o any recording. So we can change/adjust this to be any value, probably should be less 15/20?
        // As far as I'm aware I don't think there are no other uses of this rollover value other than inactivity.
        private static int ROLLOVER = 45; // 1045; // 45 

        private static readonly log4net.ILog Log = 
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const int DefaultWidth = 370;
        public const int DefaultHeight = 600;

        private static int LoginAttempt = 0;

        private static bool isOutCreditMsg = false;



        private static System.Timers.Timer aTimer;

        #endregion

        #region ReadOnly

        private readonly ISender _sender;
        private readonly ITables _tables;
        private readonly IScreenService _screenService;
        private readonly INetworkService _networkService;
        private readonly IRecordingService _recordingService;
        private readonly IWebAPIService _webAPIService;

        #endregion

        #region Fields

        private SoundPlayer startSound = new SoundPlayer("start.wav");
        private SoundPlayer stopSound = new SoundPlayer("stop.wav");
        private int status = 0;     // Not connected
        //private bool busy = false;
        private Process prSTT1 = null;
        private Process prSTT2 = null;
        private int sessTextLen1 = 0;
        private int sessTextLen2 = 0;

        private int counter1 = -1;
        private int counter2 = -1;
        private int overlap = 10;
        private int secCount = 0;

        //private int phraseCount = 0;
        private bool newParagraph = true;
        private bool newLine = false;
        private bool noSpace = true; // i.e. noSpaceBefore, I think
        //private bool noSpaceAfter = false;
        private bool capitalize = true;
        private string[] prevPhrase = new string[3];
        private int prevLen = 0;

        private IntPtr previousForegroundWindow = IntPtr.Zero;

        //private bool _isInPlaceDictation = true; // to turn on in-place/in-text-box dictation 

        private DispatcherTimer _dispatcherTimer;
        List<string> _filesList;

        #endregion

        #region Events

        public event EventHandler Started;
        public event EventHandler LoggedInChanged;

        protected void OnStarted([CallerMemberName] string propertyName = null) =>
            Started?.Invoke(this, EventArgs.Empty);

        protected void RaiseLoggedInChanged() =>
            LoggedInChanged?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Properties

        /// <summary>
        /// overrides the default to make this window non-fucusable (i.e. everything goes into the other window
        /// that we want to type in (focus, keyboard etc., mouse is handled in the wnd proc to allow brief actions), 
        /// and it makes it pretty much 'non usable' (e.g. we can't type in) but 
        /// </summary>
        //protected override CreateParams CreateParams
        //{
        //    [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        if (_isInPlaceDictation) return cp;
        //        cp.ExStyle |= (int)WS_EX_NOACTIVATE;
        //        return cp;
        //    }
        //}


        private bool _canSetIsListening = true;
        private bool? _previousIsListening = null;
        private bool _forceSetIsListening = false;
        private bool _isListening = false;
        public bool IsListening
        {
            get { return this._isListening; }
            set
            {                
                if (!_canSetIsListening)
                {
              
                    return;
                }

                if (this._isListening == value || (!IsNetworkAvailable && !_forceSetIsListening) || !IsLoggedIn)
                {
  //                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first",
  //"Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                PreviousPing = DateTime.Now;
                this._isListening = value;
                this.OnPropertyChanged(nameof(IsListening));

                // this is async but it's ok (on event basically)
                this.OnListeningChanged();

                //// this is to raise the event for app to listen and minimize the window
                //OnStarted();
            }
        }

        private bool _isNetworkAvailable = true;
        public bool IsNetworkAvailable
        {
            get { return this._isNetworkAvailable; }
            set
            {
                if (this._isNetworkAvailable == value) return;
                this._isNetworkAvailable = value;
                this.OnPropertyChanged(nameof(IsNetworkAvailable));
                this.OnPropertyChanged(nameof(NetworkRecordingStatus));
                this.OnNetworkChanged();
                this.OnNetworkOrStatusChanged();
            }
        }

        private RecordingStatus _status;
        public RecordingStatus Status
        {
            get { return this._status; }
            set
            {
                if (this._status == value) return;
                this._status = value;
                this.OnPropertyChanged(nameof(Status));
                this.OnPropertyChanged(nameof(NetworkRecordingStatus));
                this.OnStatusChanged();
                this.OnNetworkOrStatusChanged();
            }
        }

        // CanRecord
        //public NetworkRecordingStatus NetworkRecordingStatus => IsNetworkAvailable ? 
        //        EnumParser.Parse<NetworkRecordingStatus>(Status.ToString()) : NetworkRecordingStatus.NoNetwork;
        public NetworkRecordingStatus NetworkRecordingStatus
        {
            get
            {
                if (!CanRecord)
                    return NetworkRecordingStatus.NoRecording;
                return IsNetworkAvailable?
                    EnumParser.Parse<NetworkRecordingStatus>(Status.ToString()) : NetworkRecordingStatus.NoNetwork;
            }
        }

        private string _logText;
        public string LogText
        {
            get { return this._logText; }
            set
            {
                if (this._logText == value) return;
                this._logText = value;
                this.OnPropertyChanged(nameof(LogText));
                //this.OnLogText();
            }
        }

        private KeyValuePair<string, string>? _selectedLanguage;
        public KeyValuePair<string, string>? SelectedLanguage
        {
            get { return this._selectedLanguage; }
            set
            {
                if (object.Equals(this._selectedLanguage, value)) return;
                this._selectedLanguage = value;
                this.OnPropertyChanged(nameof(SelectedLanguage));
                this.OnSelectedLanguageChanged();
            }
        }

        public Dictionary<string, string> LanguagesHash { get; } = new Dictionary<string, string>();

        public int? SelectedLanguageIndex
        {
            get
            {
                int? index = null; // 0;
                if (SelectedLanguage != null)
                {
                    index = LanguagesHash
                        .Select((x, i) => new { Index = i, Value = x })
                        .FirstOrDefault(x => object.Equals(x.Value, SelectedLanguage.Value))
                        ?.Index ?? null; // 0;
                }
                return index;
            }
            set
            {
                KeyValuePair<string, string>? language = null;
                if (value != null)
                {
                    var index = value.Value;
                    if (index >= 0 && index < LanguagesHash.Count)
                    {
                        language = LanguagesHash.ToList()[index];
                    }
                }
                SelectedLanguage = language;
                this.OnPropertyChanged(nameof(SelectedLanguageIndex));
            }
        }

        public KeyValuePair<string, string> FirstLanguage =>
            LanguagesHash.FirstOrDefault();

        private int _left = 50;
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

        private int _top = 50;
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

        private int _width = 370;
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

        private int _gridWidth = 350;
        public int GridWidth
        {
            get { return this._gridWidth; }
            set
            {
                if (this._gridWidth == value) return;
                this._gridWidth = value;
                this.OnPropertyChanged(nameof(GridWidth));
            }
        }

        private int _gridHeight = 600;
        public int GridHeight
        {
            get { return this._gridHeight; }
            set
            {
                if (this._gridHeight == value) return;
                this._gridHeight = value;
                this.OnPropertyChanged(nameof(GridHeight));
            }
        }

        private bool _languagesEnabled = true;
        public bool LanguagesEnabled
        {
            get { return this._languagesEnabled; }
            set
            {
                if (this._languagesEnabled == value) return;
                this._languagesEnabled = value;
                this.OnPropertyChanged(nameof(LanguagesEnabled));
                //this.OnLanguagesEnabledChanged();
            }
        }

        private bool _canRecord = true;
        public bool CanRecord
        {
            get { return this._canRecord; }
            set
            {
                LocalSettings.canRecord = value;
                if (this._canRecord == value) return;
                this._canRecord = value;            
                this.OnPropertyChanged(nameof(CanRecord));
                this.OnPropertyChanged(nameof(NetworkRecordingStatus));
                this.OnCanRecordChanged();
            }
        }

        private bool _isMinimized = false;
        public bool IsMinimized
        {
            get { return this._isMinimized; }
            set
            {
                if (this._isMinimized == value) return;
                this._isMinimized = value;
                this.OnPropertyChanged(nameof(IsMinimized));
                this.OnIsMinimizedChanged();
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Log In Events / Properties
        public AwaitableDelegateCommand<object> LogInCommand { get; }
        public DelegateCommand LogoutCommand { get; }

        public bool CredentialsReadyToLogIn
        {
            get
            {
                // the only bug here is that SecurePassword is not set till we actually edit the password box
                // i.e. even if we have the password from registry at start
                return !string.IsNullOrWhiteSpace(Username) && SecurePassword?.Length != 0;
            }
            set
            {
            }
        }
        public bool AllowLogIn
        {
            get
            {
                return !IsLoggedIn && !string.IsNullOrWhiteSpace(Username) && SecurePassword?.Length != 0;
            }
        }

        private string _username;
        public string Username
        {
            get { return this._username; }
            set
            {
                if (this._username == value) return;
                this._username = value;
                this.OnPropertyChanged(nameof(Username));
                this.OnAllowLogInChanged();
                this.OnCredentialsChanged();
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
                this.OnAllowLogInChanged();
                this.OnCredentialsChanged();
            }
        }

        private SecureString _securePassword = new SecureString();
        public SecureString SecurePassword
        {
            private get { return this._securePassword; }
            set
            {
                if (this._securePassword.Equals(value)) return;
                this._securePassword = value;
                this.OnPropertyChanged(nameof(SecurePassword));
                this.OnAllowLogInChanged();
                this.OnCredentialsChanged();
            }
        }        
        public bool IsLoggedIn
        {
            get
            {
                return LocalSettings._isLoggedIn;
            }
            set
            {
                if (LocalSettings._isLoggedIn == value) return;
                LocalSettings._isLoggedIn = value;
                this.OnPropertyChanged(nameof(IsLoggedIn));
                this.OnAllowLogInChanged();
                this.OnPropertyChanged(nameof(NeedsLogIn));
                this.OnIsLoggedInChanged();
            }
        }
        private bool _Logedin;
        public bool Logedin
        {
            get
            {
                return _Logedin;
            }
            set
            {
                if (_Logedin == value) return;
                _Logedin = value;
                this.OnPropertyChanged(nameof(Logedin));
            }
        }

        public bool NeedsLogIn
        {
            get
            {
                return !IsLoggedIn;
            }
            set
            {
            }
        }

        private string _visibility;
        public string Visibility
        {
            get
            {
                return _visibility= IsLoggedIn == true  ? "Visible" : "Collapsed";
            }
            set
            {
               // if (this._visibility == value) return;
                this._visibility = value;
                this.OnPropertyChanged(nameof(Visibility));
            }
        }

        private string _visibility1;
        public string Visibility1
        {
            get
            {
                return _visibility1= IsLoggedIn == true ? "Collapsed" : "Visible";
            }
            set
            {
                //if (this._visibility1 == value) return;
                this._visibility1 = value;
                this.OnPropertyChanged(nameof(Visibility1));
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                if (this._email == value) return;
                this._email = value;
                this.OnPropertyChanged(nameof(Email));
            }
        }


        private string _currentcredit;
        public string Currentcredit
        {
            get
            {
                return _currentcredit;
            }
            set
            {
                if (this._currentcredit == value) return;
                this._currentcredit = value;
                this.OnPropertyChanged(nameof(Currentcredit));
            }
        }

        private string _visibilityLogin;

        public string VisibilityLogin
        {
            get
            {
                return _visibilityLogin = IsLoggedIn == false ? "Visible" : "Collapsed";
            }
            set
            {
                if (this._visibilityLogin == value) return;
                this._visibilityLogin = value;
                this.OnPropertyChanged(nameof(VisibilityLogin));
            }
        }


        //IsRememberMeOn
        private bool _isRememberMeOn;
        public bool IsRememberMeOn
        {
            get
            {
                return _isRememberMeOn;
            }
            set
            {
                if (this._isRememberMeOn == value) return;
                this._isRememberMeOn = value;
                this.OnPropertyChanged(nameof(IsRememberMeOn));
            }
        }

        public string BMToken => this.BMTokenInfo?.accessToken.token;

        private TokenInfo _bmTokenInfo;
        public TokenInfo BMTokenInfo
        {
            get { return this._bmTokenInfo; }
            set
            {
                if (this._bmTokenInfo == value) return;
                this._bmTokenInfo = value;
                this.OnPropertyChanged(nameof(BMTokenInfo));
                this.OnPropertyChanged(nameof(BMToken));
                this.OnBMTokenChanged();
            }
        }

        public string GoogleToken => this.GoogleTokenInfo.googleToken.token;

        private GoogleTokenInfo _googleTokenInfo;
        public GoogleTokenInfo GoogleTokenInfo
        {
            get { return this._googleTokenInfo; }
            set
            {
                if (this._googleTokenInfo == value) return;
                this._googleTokenInfo = value;
                this.OnPropertyChanged(nameof(GoogleTokenInfo));
                this.OnPropertyChanged(nameof(GoogleToken));
                this.OnGoogleTokenChanged();
            }
        }

        private UserDateInfo _trackingInfo;
        public UserDateInfo TrackingInfo
        {
            get { return this._trackingInfo; }
            set
            {
                if (this._trackingInfo == value) return;
                this._trackingInfo = value;
                this.OnPropertyChanged(nameof(TrackingInfo));
                this.OnGoogleTokenChanged();
            }
        }

        private DateTime _googleTokenExpiresAt = DateTime.MinValue;
        public DateTime GoogleTokenExpiresAt
        {
            get { return this._googleTokenExpiresAt; }
            set
            {
                if (this._googleTokenExpiresAt == value) return;
                this._googleTokenExpiresAt = value;
                this.OnPropertyChanged(nameof(GoogleTokenExpiresAt));
                //this.OnGoogleTokenExpiresAtChanged();
            }
        }

        private DateTime _bmTokenExpiresAt;
        public DateTime BMTokenExpiresAt
        {
            get { return this._bmTokenExpiresAt; }
            set
            {
                if (this._bmTokenExpiresAt == value) return;
                this._bmTokenExpiresAt = value;
                this.OnPropertyChanged(nameof(BMTokenExpiresAt));
                //this.OnGoogleTokenExpiresAtChanged();
            }
        }

        private int _credits;
        public int Credits
        {
            get { return this._credits; }
            set
            {
                if (this._credits == value) return;
                this._credits = value;
                this.OnPropertyChanged(nameof(Credits));
                //this.OnGoogleTokenExpiresAtChanged();
            }
        }

        private DateTime _previousPing = DateTime.MinValue;
        public DateTime PreviousPing
        {
            get { return this._previousPing; }
            set
            {
                if (this._previousPing == value) return;
                this._previousPing = value;
                this.OnPropertyChanged(nameof(PreviousPing));
                //this.OnGoogleTokenExpiresAtChanged();
            }
        }
        #endregion

        

        public MainViewModel( IScreenService screenService, INetworkService networkService, IRecordingService recordingService,IWebAPIService webAPIService)
        {
            _screenService = screenService;
            _networkService = networkService;
            _recordingService = recordingService;
            _webAPIService = webAPIService;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _dispatcherTimer = new DispatcherTimer()));
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _dispatcherTimer.Tick += new EventHandler(OnTick)));
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000)));
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _dispatcherTimer.Start()));
          
            //_sender = sender;
            _tables = new Tables();

            // => lblMonitor.Text += text
            _sender = new LowLevelSender(text => { }, _tables);

            //Control.CheckForIllegalCrossThreadCalls = false;



            // load languages (now it's in another class, init combo afterwards)
            //cbLanguages.Items.Clear();


            string errmsg = _tables.LoadLanguages();

            LocalSettings.ButtionCheck += (s, e) => 
            {
                Visibility1 = "Collapsed";
                Visibility = "Visible";
                VisibilityLogin = "Collapsed";
            };

            if (errmsg.Length > 0)
            {
                System.Windows.MessageBox.Show(errmsg +"\n" +"\n"+ "Please correct the error or get more information about configuration files on our online help page.",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Stop);
                Environment.Exit(1);
            }
            LocalSettings.StopRecordApi += async (s1, e1) =>
            {
                if (LocalSettings.isListening)
                {
                    LocalSettings.isListening = false;
                    await EnsureBMToken();
                    var tokenInfoo = await _webAPIService.UpdateTrackingStopAsync(this.BMToken);
                    this.TrackingInfo = tokenInfoo;
                    Currentcredit = tokenInfoo != null ? tokenInfoo.user.credits.ToString() + " min" : Currentcredit;
                }
            };
            LanguagesHash = _tables.LanguagesHash;
            LogInCommand = new AwaitableDelegateCommand<object>(
                async param =>
                {
                    if (param != null)
                    {
                        if (!CredentialsReadyToLogIn) return;
                    }

                    else if (!AllowLogIn)
                    {
                        System.Windows.MessageBox.Show("Can’t log-in without credentials " + "\n" + "\n" + "Please enter your email and password to log-in.", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return;
                    }
                    var tokenInfo = await _webAPIService.GetBMTokenAsync(Username, SecureService.SecureStringToString(SecurePassword));
                    Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                    var s =   ProcessLogIn(tokenInfo);

                    if (s != null && s.user.name != "" && s.user.name != null)
                    {
                        Email = s.user.name;
                        Currentcredit = s.user.credits.ToString() + " min";
                        _filesList = new List<string>();
                        _filesList.Add(Resources.LoginSuccess);
                        LoginAttempt = 0;
                        var p = IsLoggedIn;
                        LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty);
                    }
                    // only save the credentials when log in is made through GUI (i.e. we have the SecurePassword set).
                    // AppConfigDefault.RememberMe = this.IsRememberMeOn;/////////////////////
                    if (Username != Properties.Settings.Default.UserName && IsLoggedIn && IsRememberMeOn&& Properties.Settings.Default.UserName!="")
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(BlueMariaLocalization.Properties.Resources.UpdateYourLoginCredential, 
                            "Blue-Maria", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Properties.Settings.Default.IsRememberMeOn = this.IsRememberMeOn;
                            Properties.Settings.Default.UserName = Username;
                            var passwrd = Encrypt(SecureService.SecureStringToString(SecurePassword));
                            LocalSettings.PassWordd = SecureService.SecureStringToString(SecurePassword);
                            LocalSettings.UserName = Username;
                            Properties.Settings.Default.Password = passwrd;
                            Properties.Settings.Default.Save();
                        }
                    }
                    else if (IsLoggedIn)
                    {
                        Properties.Settings.Default.IsRememberMeOn = this.IsRememberMeOn;
                        if (this.IsRememberMeOn)
                        {
                            LocalSettings.PassWordd = SecureService.SecureStringToString(SecurePassword);
                            LocalSettings.UserName = Username;
                            Properties.Settings.Default.UserName = Username;
                           //LocalSettings.UserName = Username;
                            var passwrd = Encrypt(SecureService.SecureStringToString(SecurePassword));
                            Properties.Settings.Default.Password = passwrd;
                            //AppConfigDefault.UserName = Username;///////
                            // skip this converting and save to registry the secure string directly, somehow
                            //AppConfigDefault.Password = SecureService.SecureStringToString(SecurePassword);////////////
                        }
                        Properties.Settings.Default.Save();
                    }
                    //await LogInAsync(Username, SecureService.SecureStringToString(SecurePassword));
                    //await LogIn();
                },
                param => true); // AllowLogIn);
            LogoutCommand = new DelegateCommand(Logout);
            // Terminate hanging instances of LstnMic / STTmic
            Process[] prev = Process.GetProcessesByName("LstnMic"); // STTmic");
            foreach (Process p in prev) p.Kill();

            prSTT1 = new Process();
            prSTT1.StartInfo.FileName = "LstnMic.exe"; // STTmic.exe";

            // # API_SWITCHED_OFF: just comment this line to remove default credentials (and remove the .json)
            //prSTT1.StartInfo.EnvironmentVariables["GOOGLE_APPLICATION_CREDENTIALS"] = "Any-App-Dictation v1-5cc9c575f099.json"; // "PP-Speed-f252b9a4cde9.json";

            prSTT1.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            prSTT1.StartInfo.CreateNoWindow = true;
            prSTT1.StartInfo.UseShellExecute = false;
            prSTT1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            prSTT1.StartInfo.RedirectStandardOutput = true;
            prSTT1.StartInfo.RedirectStandardInput = true;
            prSTT1.OutputDataReceived += new DataReceivedEventHandler(DataRecieved);

            // DRAGAN: we don't need 2 LstnMic processes running, we have just one now. The easiest (dirty) fix was to just
            // have a 'dummy' 2nd process (which is the same as 1st), that wasy we don't have to change any code. We only need
            // to avoid kiling or starting processes (which is done below)

            prSTT2 = prSTT1;

            //this.Loaded

            // moved here before we're drawn
            LoadConfig();

            // this.IsRememberMeOn = AppConfigDefault.RememberMe;////////////////////
            this.IsRememberMeOn = Properties.Settings.Default.IsRememberMeOn;
            if (this.IsRememberMeOn)
            {
                // skip this plain password and send the encrypted secure string to the server directly somehow
                ///// Username = AppConfigDefault.UserName;/////////
                Username = Properties.Settings.Default.UserName;
                var pswrd = Properties.Settings.Default.Password;
                var pass = Decrypt(pswrd);
                LocalSettings.UserName = Username;
                LocalSettings.PassWordd = pass;
                
                ///var pass = AppConfigDefault.Password;//////////
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pass))
                { }
                else
                {
                    try { 
                    // WARNING: never do this (wait), but this is at the very start, no locks or anything here yet, should be ok?
                    // this is required to get the synchronous right
                    var tokenInfo = Task.Run(() => _webAPIService.GetBMTokenAsync(Username, pass)).Result;
                 var s=   ProcessLogIn(tokenInfo);
                        if (s != null && s.user.name != "" && s.user.name != null)
                        {
                           Email = s.user.name;
                            Currentcredit = Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                            // only save the credentials when log in is made through GUI (i.e. we have the SecurePassword set).
                            // i.e. this is an automated log in at startup, nothing to save here, we've just loaded 
                            // (and SecurePassword is empty!)
                            //var info = LogIn(Username, pass);
                            // skip this converting and save to registry the secure string directly, somehow
                            //AppConfigDefault.Password = SecureService.SecureStringToString(SecurePassword);
                        }
                    }
                    catch
                    {
                        _isNetworkAvailable = false;
                        Status = RecordingStatus.InError;
                        LocalSettings.ChangeIcon("Red");
                    }
                }
            }
            if (!this.IsLoggedIn) this.IsMinimized = false;
            var hwid = FingerPrint.Value();
        }

        public static string Encrypt(string text)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public  string Decrypt(string cipher)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }

        public string DecodeFrom64(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        public static string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        } //this function Convert to Decord your Password
        
        public async void Logout()
        {
            if (await InternetCheck.Internet())
            {

                LocalSettings.PasswrdLogout?.Invoke(this, EventArgs.Empty);
                _filesList = new List<string>();
                _filesList.Add(Resources.Logoutsuccess);
                LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty);

               await Task.Run(() => _webAPIService.UpdateBMTokenLogoutAsync(this.BMToken));
                this.IsLoggedIn = false;
                if (CanRecord)
                {
                    LocalSettings.ChangeIcon("Grey");
                }
                else
                {
                    LocalSettings.ChangeIcon("Red");
                }
                LocalSettings.LoginCheck = false;
                Logedin = false;
                // Username = "";
                Visibility = "Collapsed";
                Visibility1 = "Visible";
                VisibilityLogin = "Visible";
            }           
        }

        private TokenInfo ProcessLogIn(TokenInfo tokenInfo)
        {
            //if (!AllowLogIn) return null;
            // "hakif@daabox.com", "12345678"
            // rework this and send some raw string to the server w/o converting to String
            // also track time/expiration
            //this.BMTokenInfo = await _webAPIService.GetBMTokenAsync(
            //    Username,
            //    SecureService.SecureStringToString(SecurePassword));

            // set this also on log out - and expiration, invoke ticker to watch and check the expirations (BM and google)
            // plus the credits
            this.IsLoggedIn = false;
            this.Logedin = false;
            this.BMTokenInfo = tokenInfo; // _webAPIService.GetBMTokenAsync(username, password).Result;

            if (this.BMTokenInfo == null)
            {              
                Log.Error($"Login failed...{ Username }");
                return null;
            }
            DateTime expiresAt;
            if (!DateTime.TryParse(this.BMTokenInfo.accessToken.expiresAt, out expiresAt))
            {
                Log.Error($"returned BM expiresAt datetime format is wrong?...{ this.BMTokenInfo.accessToken.expiresAt }");
                return null;
            }
            this.BMTokenExpiresAt = expiresAt;
            PreviousPing = DateTime.Now;
            if (CanRecord)
            {
                LocalSettings.ChangeIcon("HalfGreen");
            }
            else
            {
                LocalSettings.ChangeIcon("Red");
            }
            this.IsLoggedIn = true;
            Logedin = true;
            LocalSettings.LoginCheck = true;
            LocalSettings.ButtionCheck?.Invoke(this, EventArgs.Empty);
            //AppConfigDefault.RememberMe = this.IsRememberMeOn;
            //if (this.IsRememberMeOn)
            //{
            //    AppConfigDefault.UserName = Username;
            //    // skip this converting and save to registry the secure string directly, somehow
            //    AppConfigDefault.Password = SecureService.SecureStringToString(SecurePassword);
            //}

            return this.BMTokenInfo;
        }

        //private async Task<TokenInfo> LogInAsync(string username, string password)
        //{
        //    //if (!AllowLogIn) return null;
        //    // "hakif@daabox.com", "12345678"
        //    // rework this and send some raw string to the server w/o converting to String
        //    // also track time/expiration
        //    //this.BMTokenInfo = await _webAPIService.GetBMTokenAsync(
        //    //    Username,
        //    //    SecureService.SecureStringToString(SecurePassword));
        //    this.BMTokenInfo = await _webAPIService.GetBMTokenAsync(username, password);

        //    if (this.BMTokenInfo == null)
        //    {
        //        Log.Error($"Login failed...{ Username }");
        //        return null;
        //    }
        //    //this.BMToken = this.BMTokenInfo.accessToken.token;
        //    //bmTokenInfo.accessToken.expiresAt;
        //    DateTime expiresAt;
        //    if (!DateTime.TryParse(this.BMTokenInfo.accessToken.expiresAt, out expiresAt))
        //    {
        //        Log.Error($"returned BM expiresAt datetime format is wrong?...{ this.BMTokenInfo.accessToken.expiresAt }");
        //        // handle this
        //        return null;
        //    }
        //    this.BMTokenExpiresAt = expiresAt;
        //    this.IsLoggedIn = true;
        //    AppConfigDefault.RememberMe = this.IsRememberMeOn;
        //    if (this.IsRememberMeOn)
        //    {
        //        AppConfigDefault.UserName = Username;
        //        // skip this converting and save to registry the secure string directly, somehow
        //        AppConfigDefault.Password = SecureService.SecureStringToString(SecurePassword);
        //    }
        //    return this.BMTokenInfo;
        //}

        //private TokenInfo LogIn(string username, string password)
        //{
        //    //if (!AllowLogIn) return null;
        //    // "hakif@daabox.com", "12345678"
        //    // rework this and send some raw string to the server w/o converting to String
        //    // also track time/expiration
        //    //this.BMTokenInfo = await _webAPIService.GetBMTokenAsync(
        //    //    Username,
        //    //    SecureService.SecureStringToString(SecurePassword));

        //    // WARNING: never do this (wait), but this is at the very start, no locks or anything here yet, should be ok?
        //    this.BMTokenInfo = _webAPIService.GetBMTokenAsync(username, password).Result;

        //    if (this.BMTokenInfo == null)
        //    {
        //        Log.Error($"Login failed...{ Username }");
        //        return null;
        //    }
        //    //this.BMToken = this.BMTokenInfo.accessToken.token;
        //    //bmTokenInfo.accessToken.expiresAt;
        //    DateTime expiresAt;
        //    if (!DateTime.TryParse(this.BMTokenInfo.accessToken.expiresAt, out expiresAt))
        //    {
        //        Log.Error($"returned BM expiresAt datetime format is wrong?...{ this.BMTokenInfo.accessToken.expiresAt }");
        //        // handle this
        //        return null;
        //    }
        //    this.BMTokenExpiresAt = expiresAt;
        //    this.IsLoggedIn = true;
        //    AppConfigDefault.RememberMe = this.IsRememberMeOn;
        //    if (this.IsRememberMeOn)
        //    {
        //        AppConfigDefault.UserName = Username;
        //        // skip this converting and save to registry the secure string directly, somehow
        //        AppConfigDefault.Password = SecureService.SecureStringToString(SecurePassword);
        //    }
        //    return this.BMTokenInfo;
        //}

        #region Property Changed Events

        private void OnIsMinimizedChanged()
        {


        }

        private void OnCanRecordChanged()
        {
            if (!CanRecord)
            {
                try
                {
                    _forceSetIsListening = true;

                    _previousIsListening = IsListening;
                    IsLoggedIn = false;
                    IsListening = false;
                }
                finally
                {
                    _forceSetIsListening = false;
                }
            }
            else 
            {           
               _previousIsListening = null;
            }
        }

        private void OnSelectedLanguageChanged()
        {
        }

        private void OnNetworkChanged()
        {
            if (!IsNetworkAvailable)
            {
                // turn off any ongoing connection as the streaming will fail even if connection recovers
                // (that was the bug, we stay 'green' but recording no longer is working in such a case)
                try
                {
                    // IsListening is not allowed to be set/reset during 'no connection', so we have to force it
                    // for manual cases like this (it's still not allowed to be set via UI)
                    _forceSetIsListening = true;

                    _previousIsListening = IsListening;
                    IsLoggedIn = false;
                    IsListening = false;
                }
                finally
                {
                    _forceSetIsListening = false;
                }
            }
            else // network is back on
            {
                // this can turn the recording back on, but not sure we want that, it might be hours after
                // we disconnected ? Just uncomment if needed

                //if (_previousIsListening == true)
                //{
                //    IsListening = true;
                //}

                _previousIsListening = null;
            }
        }

        private void OnStatusChanged()
        {
        }

        private void OnNetworkOrStatusChanged()
        {
        }

        //private void OnListeningChanged()
        private async void OnListeningChanged()
        {
            try
            {
                _canSetIsListening = false;
                var sr = LocalSettings.canRecord;
                if (LocalSettings.isListening)
                {
                    if (!IsListening)
                    {

                        var r = LocalSettings.canRecord;

                        _dispatcherTimer.Stop();


                        KillSTT(prSTT1);
                        KillSTT(prSTT2);

                        status = 0;
                        counter1 = -1;
                        counter2 = -1;

                        _tables.ClearTables();

                        LanguagesEnabled = true;

                        stopSound.Play();

                        Status = RecordingStatus.InActive;
                        LocalSettings.ChangeIcon("HalfGreen");
                        if (await InternetCheck.Internet())
                        {
                            await EnsureBMToken();
                            var tokenInfo = await _webAPIService.UpdateTrackingStopAsync(this.BMToken);
                            this.TrackingInfo = tokenInfo;
                            Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                        }
                        _dispatcherTimer.Start();
                        LocalSettings.isListening = false;
                    }
                }
                else if(!LocalSettings.IsCreditZero)
                {

                    if (CanRecord && IsNetworkAvailable)
                    {
                        LanguagesEnabled = false;
                        PreviousPing = DateTime.Now;

                        var index = SelectedLanguageIndex;
                        var language = SelectedLanguage ?? LanguagesHash.FirstOrDefault();

                        status = 1;

                        string lang = language.Key;
                        //string lang = _tables.Languages.ToArray()[cbLanguages.SelectedIndex];
                        _dispatcherTimer.Stop();

                        string errmsg = _tables.SetupTables(lang);
                        
                        if (errmsg.Length > 0)
                        {
                            System.Windows.MessageBox.Show(errmsg + "\n" + "\n" + "Please correct the error or get more information about configuration files on our online help page.", 
                                "Blue-Maria", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Stop);
                            //_filesList = new List<string>();
                            //_filesList.Add("Configuration error");
                            //LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty);  
                            Environment.Exit(1);
                        }
                        _dispatcherTimer.Start();
                        Status = RecordingStatus.Gif;
                        await Task.Delay(3000);
                        newParagraph = false;
                        noSpace = false; // true;
                        
                        // TODO: think of something better, we should do it async? but also to avoid
                        // duplicating requests to LIstening
                        // NOTE: we're calling API to fetch token on each 'start', this is making the
                        // start rather slow (and needs some visual response, hourglass or something)
                        // but it shouldn't be normally as server should create tokens ahead of time

                        // # API_SWITCHED_OFF: just remove the temp token ("111...") and let API get it
                        //var token = "111"; // await _webAPIService.GetGoogleToken("username", "password"); //.Result
                        //var token = await _webAPIService.GetGoogleTokenAsync("hakif@daabox.com", "12345678");

                        await EnsureBMToken();
                        if (!IsLoggedIn || string.IsNullOrWhiteSpace(this.BMToken))
                        {
                            //_filesList = new List<string>();
                            //_filesList.Add("Not logged in.");
                            //LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty);
                            Status = RecordingStatus.InActive;
                            LanguagesEnabled = true;
                            Log.Error($"Trying to start tracking but we're not logged in?...{ this.BMToken }");
                            return;
                        }

                        GoogleTokenInfo tokenInfo = this.GoogleTokenInfo;
                        // even if it didn't expire we need to call the tracking/start to start tracking, we'll get the same or new token depending on its status.
                        //if (this.GoogleTokenExpiresAt <= DateTime.Now)
                        {
                            try
                            {
                                if (await InternetCheck.Internet())
                                {
                                    tokenInfo = await _webAPIService.GetTrackingAsync(this.BMToken);
                                    this.GoogleTokenInfo = tokenInfo;
                                    Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                                    if (this.GoogleTokenInfo!=null&&GoogleTokenInfo.user.credits == 0 )
                                    {
                                        LanguagesEnabled = true;
                                        Status = RecordingStatus.InActive;
                                        LocalSettings.ApiRecordingStatus = "Stop";
                                        LocalSettings.IsCreditZero = true;
                                        MessageBoxResult dresult = System.Windows.MessageBox.Show("Dictation not possible without credits, please purchase additional credits on our homepage.", 
                                                                            "Blue-Maria", 
                                                                            MessageBoxButton.OK, 
                                                                            MessageBoxImage.Error);
                                        isOutCreditMsg = true;
                                        if (dresult == MessageBoxResult.OK)
                                        {
                                            isOutCreditMsg = false;
                                        }

                                        return;
                                    }
                                    if (this.GoogleTokenInfo == null)
                                    {
                                        //_filesList = new List<string>();
                                        //_filesList.Add("tracking failed");
                                        //LocalSettings.ToastNotification?.Invoke(_filesList, EventArgs.Empty);
                                        if (Currentcredit =="0")
                                        {
                                            LanguagesEnabled = true;
                                            LocalSettings.IsCreditZero = true;
                                            LocalSettings.ApiRecordingStatus = "Stop";
                                            Status = RecordingStatus.InActive;
                                            MessageBoxResult dresult = System.Windows.MessageBox.Show("Dictation not possible without credits, please purchase additional credits on our homepage.", 
                                                                            "Blue-Maria", 
                                                                            MessageBoxButton.OK, 
                                                                            MessageBoxImage.Error);
                                            isOutCreditMsg = true;
                                            if (dresult == MessageBoxResult.OK)
                                            {
                                                isOutCreditMsg = false;
                                            }
                                            return;
                                        }

                                        LanguagesEnabled = true;
                                        Status = RecordingStatus.InActive;
                                        Log.Error($"Getting google token (tracking) failed...{ this.BMToken }");
                                        return;
                                    }
                                    if (this.GoogleTokenInfo.googleToken == null)
                                    {
                                        LanguagesEnabled = true;
                                        Status = RecordingStatus.InActive;
                                        LocalSettings.ApiRecordingStatus = "Stop";
                                        // System.Windows.MessageBox.Show($"strange server response but indicating that we're out of credits, google token failed...{ this.BMToken }, credits: { tokenInfo.user.credits }", "Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    DateTime expiresAt;
                                    if (!DateTime.TryParse(tokenInfo.googleToken.expiresAt, out expiresAt))
                                    {
                                        //  System.Windows.MessageBox.Show($"strange server response but indicating that we're out of credits, google token failed...{ this.BMToken }, credits: { tokenInfo.user.credits }", "Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);
                                        LanguagesEnabled = true;
                                        Status = RecordingStatus.InActive;
                                        LocalSettings.ApiRecordingStatus = "Stop";
                                        Log.Error($"returned expiresAt datetime format is wrong?...{ tokenInfo.googleToken.expiresAt }");
                                        // handle this
                                        return;
                                    }
                                    this.GoogleTokenExpiresAt = expiresAt;
                                    this.Credits = tokenInfo.user.credits;

                                    // we should refresh the expiring token if that's the problem, but on the other hand we 
                                    // we shouldn't be getting a token like that from the server in the first place
                                    if (this.GoogleTokenExpiresAt < DateTime.Now.AddMinutes(1) || this.Credits < 1)
                                    {

                                        Log.Error($"we're out of credits or token is expiring...{ this.BMToken }, credits: { this.Credits }, expires: {this.GoogleTokenExpiresAt}, now: {DateTime.Now}");
                                        // don't exit right away, let it unwind to collect some more data...
                                        //success = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    LanguagesEnabled = true;
                                    LocalSettings.ApiRecordingStatus = "Stop";
                                    Status = RecordingStatus.InActive;
                                    return;
                                }
                                //expiresAt = expiresAt.ToUniversalTime();
                            }
                            catch (Exception e)
                            {
                                MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong. We apologize!" + "\n" + "Please restart the application and try again. If the issue persists and you are able to reproduce it, then please click 'Yes' to report your findings", 
                                                                "Blue-Maria", 
                                                                MessageBoxButton.YesNo, 
                                                                MessageBoxImage.Error);
                                if (result == MessageBoxResult.Yes)
                                {
                                    System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                                }
                                Log.Error("getting google token error...", e);
                                return;
                            }
                        }
                        //tokenInfo.googleToken.expiresAt;
                        //tokenInfo.user.credits;
                        var token = tokenInfo.googleToken.token;
                        token = token.Trim().Trim('\r', '\n', ' ');
                        lang = string.Join("", lang.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                        var arguments = $"{lang} {token}"; //@;@
                        prSTT1.StartInfo.Arguments = arguments; // lang;
                        prSTT2.StartInfo.Arguments = arguments; // lang;

                        counter1 = ROLLOVER;
                        counter2 = -1;
                        overlap = 10;
                        secCount = 0;

                        prSTT1.Start();
                        prSTT1.BeginOutputReadLine();

                        _dispatcherTimer.Start();


                        LocalSettings.isListening = true;
                        LocalSettings.StopRecording = true;
                        //timer.Enabled = true;

                        //  Status = RecordingStatus.Gif;
                        //lblStartStop.Image = Properties.Resources.mic_orange;
                    }
                }
                        //busy = !busy;

               
               
            }
            catch (Exception e)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong. We apologize!" + "\n" + "Please restart the application and try again. If the issue persists and you are able to reproduce it, then please click 'Yes' to report your findings", 
                                                    "Blue-Maria", 
                                                    MessageBoxButton.YesNo, 
                                                    MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                }
                Log.Error(e.Message, e);
                // TODO: token fetch failed, we should do something, abort IsListening or something
            }
            finally
            {
                _canSetIsListening = true;

                //// this is to raise the event for app to listen and minimize the window
                //OnStarted();
            }
        
        }

        private void OnAllowLogInChanged()
        {
            this.OnPropertyChanged(nameof(AllowLogIn));
            LogInCommand.RaiseCanExecuteChanged();
            this.OnPropertyChanged(nameof(CredentialsReadyToLogIn));
        }

        private void OnCredentialsChanged()
        {
            //AllowLogIn = !string.IsNullOrWhiteSpace(Username) && SecurePassword?.Length != 0;
        }

        private void OnBMTokenChanged()
        {
        }

        private void OnIsLoggedInChanged()
        {
            RaiseLoggedInChanged();
        }

        private void OnGoogleTokenChanged()
        {
        }

        #endregion

        #region Timer

        /// <summary>
        /// this does very little now, but it serves some function, we need to rollover counters and handle idle time etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTick(object sender, EventArgs e)
        {
            //if (IsLoggedIn)
            //{
            //    await App.Current.Dispatcher.Invoke(async () =>
            //    {
            //        var s = await _webAPIService.GetTrackingAsync(BMToken);
            //    });
            //}
            if (LocalSettings.IsCreditZero)
            {
                _canSetIsListening = true;
                IsListening = false;
                LocalSettings.IsCreditZero = false;
                
                
                

                // await  System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(() => IsListening = false));
                
            }

           
            if (LocalSettings.WindowShrinked)
            {
                LocalSettings.LoadShrikedPageDatacontext?.Invoke(this, EventArgs.Empty);
                LocalSettings.WindowShrinked = false;
            }
            if (LocalSettings.IsMainpageLoaded)
            {
                LocalSettings.LoadDataContext?.Invoke(this, EventArgs.Empty);
                LocalSettings.IsMainpageLoaded = false;

            }
            if (LocalSettings.Netcheck &&await InternetCheck.Internet())
            {
               await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { LocalSettings.StopRecordApi?.Invoke(this, EventArgs.Empty); }));
                LocalSettings.Netcheck = false;
            }

            if (PreviousPing + _pingInterval < DateTime.Now && IsLoggedIn && IsListening)
            {
                // not sure but it's probably safer, though we shouldn't have any major interaction, just to ping,
                // but still we'd want to save the time, credits and that may set the properties tracked by GUI
                await App.Current.Dispatcher.Invoke(async () =>
                {
                    await OnPing();
                });
            }

            secCount++;
            if (counter1 == 0 && prSTT1 != null)
            {
                // DRAGAN: we're no longer killing on rollover. Also the 'if (...)' below should've been part of the killSTT as it's 
                // always needed (better design). And is this is a 'soft kill' we're not stopping, nor begging the standard output
                KillSTT(prSTT1, forReal: false);
            }
            if (counter2 == 0 && prSTT2 != null)
            {
                // DRAGAN: same as the above just for the other process, some sort of semaphore is better here (but we're keeping the old code, min changes)
                KillSTT(prSTT2, forReal: false);
            }

            if (counter1 == overlap)
            {
                counter2 = ROLLOVER;
                if (sessTextLen1 > 0)
                {
                    // DRAGAN: we're no longer starting nor killing processes on rollover
                    //prSTT2.Start();
                    sessTextLen2 = 0;
                    secCount = 0;
                }
                else
                    IsListening = !IsListening;
                //OnListeningChanged();
                //LblStartStop_Click(null, null);
            }
            if (counter2 == overlap)
            {
                counter1 = ROLLOVER;
                if (sessTextLen2 > 0)
                {
                    // DRAGAN: we're no longer starting nor killing processes on rollover
                    //prSTT1.Start();
                    sessTextLen1 = 0;
                    secCount = 0;
                }
                else
                    IsListening = !IsListening;
            }

            if (counter1 >= 0) counter1--;
            if (counter2 >= 0) counter2--;
        }

        private async Task OnPing()
        {
            if (!IsLoggedIn || !IsListening) return;

            await EnsureBMToken();
            var tokenInfo = await _webAPIService.UpdateTrackingPingAsync(this.BMToken);
            this.TrackingInfo = tokenInfo;
            this.Credits = tokenInfo?.user.credits ?? 0;
            Currentcredit = tokenInfo != null? this.Credits.ToString()+ " min" : Currentcredit;
            PreviousPing = DateTime.Now;

            // we should refresh the expiring token if that's the problem, but on the other hand we 
            // we shouldn't be getting a token like that from the server in the first place
            if (tokenInfo == null || this.GoogleTokenExpiresAt < DateTime.Now.AddMinutes(2)  )
            {
                if (this.Credits < 1)
                {
                    IsListening = false;
                    if (!isOutCreditMsg)
                    {
                        System.Windows.MessageBox.Show("You do not have any credits left. Please purchase additional credits on our homepage", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                    }
                    Log.Error($"OnPing: we're out of credits or token is expiring...{ this.BMToken }, credits: { this.Credits }, expires: {this.GoogleTokenExpiresAt}, now: {DateTime.Now}");
                    // don't exit right away, let it unwind to collect some more data...
                    //success = false;
                }
                if (this.Credits >= 1)
                {
                    try
                    {
                        // meaning we could be fine just to refresh the token...
                        var refreshTokenInfo = await _webAPIService.UpdateGoogleTokenRefreshAsync(this.BMToken);
                        if (refreshTokenInfo == null)
                        {
                            Log.Error($"Refreshing google token failed...{ this.BMToken }");
                            return;
                        }
                        if (refreshTokenInfo.googleToken == null)
                        {                            
                            System.Windows.MessageBox.Show("You do not have any credits left. Please purchase additional credits on our homepage", 
                                "Blue-Maria", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                            Log.Error($"strange server response but indicating that we're out of credits, google token refresh failed...{ this.BMToken }, credits?: { this.Credits }");
                            return;
                        }
                        this.GoogleTokenInfo = new GoogleTokenInfo
                        {
                            date = refreshTokenInfo.date,
                            googleToken = refreshTokenInfo.googleToken,
                            user = null,
                        };

                        DateTime expiresAt;
                        if (!DateTime.TryParse(this.GoogleTokenInfo.googleToken.expiresAt, out expiresAt))
                        {
                            
                            Log.Error($"returned expiresAt datetime format is wrong?...{ this.GoogleTokenInfo.googleToken.expiresAt }");
                            // handle this
                            return;
                        }
                        this.GoogleTokenExpiresAt = expiresAt;

                        // and now do the ping again to get the user/credits info, if it works
                        tokenInfo = await _webAPIService.UpdateTrackingPingAsync(this.BMToken);
                        this.TrackingInfo = tokenInfo;
                        this.Credits = tokenInfo?.user.credits ?? 0;
                        Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                        PreviousPing = DateTime.Now;

                        if (this.GoogleTokenExpiresAt < DateTime.Now.AddMinutes(1) || this.Credits < 1)
                        {
                            System.Windows.MessageBox.Show("You do not have any credits left. Please purchase additional credits on our homepage", 
                                "Blue-Maria", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                            Log.Error($"Ping: we're out of credits or token is expiring...{ this.BMToken }, credits: { this.Credits }, expires: {this.GoogleTokenExpiresAt}, now: {DateTime.Now}");
                            // don't exit right away, let it unwind to collect some more data...
                            //success = false;
                            return;
                        }

                    }
                    catch (Exception e)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong. We apologize!" + "\n" + "Please restart the application and try again. If the issue persists and you are able to reproduce it, then please click 'Yes' to report your findings", 
                                                        "Blue-Maria", 
                                                        MessageBoxButton.YesNo, 
                                                        MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                        }
                        Log.Error("ping: ", e);
                    }
                }
                return;
            }
        }

        private async Task<bool> EnsureBMToken()
        {
            if (BMTokenExpiresAt > DateTime.Now.AddMinutes(5)) return true;

            BMTokenDateInfo info = await _webAPIService.UpdateBMTokenRefreshAsync(this.BMToken);
            if (info == null)
            {
                
                Log.Error($"BM token refresh failed...{ Username }");

                Username = AppConfigDefault.UserName;
                var pass = AppConfigDefault.Password;
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pass))
                    return false;

                var tokenInfo = await _webAPIService.GetBMTokenAsync(Username, pass);
                Currentcredit = tokenInfo != null ? tokenInfo.user.credits.ToString() + " min" : Currentcredit;
                ProcessLogIn(tokenInfo);
                if (!IsLoggedIn)
                    return false;
                return true;
            }

            var user = this.BMTokenInfo?.user;
            this.BMTokenInfo = new TokenInfo
            {
                date = info.date,
                accessToken = info.accessToken,
                user = user, // we have nothing better till the next ping or update
            };

            this.IsLoggedIn = false;

            DateTime expiresAt;
            if (!DateTime.TryParse(this.BMTokenInfo.accessToken.expiresAt, out expiresAt))
            {
             
                Log.Error($"returned BM expiresAt datetime format is wrong?...{ this.BMTokenInfo.accessToken.expiresAt }");
                // handle this
                return false;
            }

            this.BMTokenExpiresAt = expiresAt;
            this.IsLoggedIn = true;
            LocalSettings.LoginCheck = true;
            return true;
        }

        #endregion

        #region Events

        public async Task OnLoaded()
        {

            IsNetworkAvailable = await _networkService.GetIsAvailable();
            CanRecord = _recordingService.GetCanRecord();

            if (IsNetworkAvailable && CanRecord)
            {

                LocalSettings.ChangeIcon("Grey");

            }
            else
            {
                LocalSettings.ChangeIcon("Red");
            }

            IsLoggedIn = CanRecord == false ? CanRecord : IsLoggedIn;
            IsLoggedIn = IsNetworkAvailable == false ? IsNetworkAvailable : IsLoggedIn;
          // IsLoggedIn = Properties.Settings.Default.IsRememberMeOn == false ? false : IsLoggedIn;
            // this isn't working, we need another layout/template for the 'shrunk window'
            //Width = 70;
            //Height = 100;
            //GridWidth = 140;
            //GridHeight = 100;

            //LoadConfig();
        }

        public void OnClosing()
        {
            if (IsListening) // if (busy)
            {
                KillSTT(prSTT1);
                KillSTT(prSTT2);
            }

            SaveConfig();
        }

        #endregion

        #region Configuration

        void LoadConfig()
        {
            if (File.Exists(BlueMaria.Properties.Settings.Default.AppData + @"\BlueMaria.cfg"))
            {
                int index;
                bool isMinimized;
                string[] cfg = File.ReadAllLines(BlueMaria.Properties.Settings.Default.AppData + @"\BlueMaria.cfg");
                for (int i = 0; i < cfg.Length; i++)
                {
                    if (cfg[i].Trim() == "") continue;

                    string[] entry = cfg[i].Split('=');
                    if (entry.Length < 2) { Log.Error($"config error: {cfg[i]}"); continue; }

                    string key = entry[0].Trim();
                    string value = entry[1].Trim();
                    switch (key)
                    {
                        case "X":
                            this.Left = Convert.ToInt32(value);
                            break;
                        case "Y":
                            this.Top = Convert.ToInt32(value);
                            break;
                        case "Language":
                            if (int.TryParse(value, out index))
                            {
                                this.SelectedLanguageIndex = index;
                            }
                            break;
                        case "IsMinimized":
                            //cfg += $"IsMinimized={IsMinimized}\n";
                            if (bool.TryParse(value, out isMinimized))
                            {
                                this.IsMinimized = isMinimized;
                            }
                            break;
                    }


                    //if (entry[0] == "X") this.Left = Convert.ToInt32(entry[1]);
                    //if (entry[0] == "Y") this.Top = Convert.ToInt32(entry[1]);
                    //if (entry[0] == "Language")
                    //{
                    //    if (Int32.TryParse(entry[1], out int index))
                    //    {
                    //        this.SelectedLanguageIndex = index;
                    //        //if (index >= 0 && index < LanguagesHash.Count)
                    //        //{
                    //        //    var language = LanguagesHash.ToList()[index];
                    //        //    SelectedLanguage = language;
                    //        //}
                    //    }
                    //    //int index = Convert.ToInt32(entry[1]);
                    //    //if(index < 0)
                    //    //var language = LanguagesHash.ToList()[index];

                    //    //SelectedLanguage = LanguagesHash.FirstOrDefault(x => x.Key )
                    //    ////cbLanguages.SelectedIndex = Convert.ToInt32(entry[1]);
                    //}
                }
            }
            else
            {
                this.Left = 50;
                this.Top = 50;
                SelectedLanguage = LanguagesHash.FirstOrDefault();
                //cbLanguages.SelectedIndex = 0;
            }
        }

        void SaveConfig()
        {
            string cfg = "";
            cfg += "X=" + this.Left + "\n";
            cfg += "Y=" + this.Top + "\n";

            var index = this.SelectedLanguageIndex ?? 0;

            //int index = 0;
            //if (SelectedLanguage != null)
            //{
            //    index = LanguagesHash
            //        .Select((x, i) => new { Index = i, Value = x })
            //        .FirstOrDefault(x => object.Equals(x.Value, SelectedLanguage.Value))
            //        ?.Index ?? 0;
            //}

            cfg += "Language=" + index + "\n";
            //cfg += "Language=" + cbLanguages.SelectedIndex + "\n";

            cfg += $"IsMinimized={IsMinimized}\n";

            File.WriteAllText(BlueMaria.Properties.Settings.Default.AppData + @"\BlueMaria.cfg", cfg);
        }

        #endregion

        #region Data Receiving

        private void DataRecieved(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            string data = e.Data.ToString().Trim();

            LogText = "|" + data.Replace("|", "") + "|";
            //lblMonitor.Text = "|" + data.Replace("|", "") + "|";

            if (status == 2)
            {
                data = data.Trim();

                if (data == ";;OK;;") return;  // Or else it will appear in the text for every new connection

                data = data.Replace("|", "");
                if (data.Length == 0) return;

                #region Prev / Next phrases - this should go out completely

                string[] newPhrase = data.Split(' ');

                // DRAGAN: just some debug dump, it can stay as it's only used in/when debugging
                Debug.WriteLine($"data: {data}, '{DateTime.Now.ToString()}'");

                if (prevLen > 0)
                {
                    int n = 3;
                    if (n > newPhrase.Length) n = newPhrase.Length;
                    if (n > prevLen) n = prevLen;
                    while (n > 0)
                    {
                        bool match = true;
                        for (int i = 0; i < n; i++)
                        {
                            if (prevPhrase[3 - n + i] == null) { match = false; break; }
                            if (newPhrase[i].ToLower() != prevPhrase[3 - n + i].ToLower())
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            newPhrase = newPhrase.Skip(n).ToArray();

                            // DRAGAN: this is a quick fix to skip 3-words processing materializing, when I have the time
                            // I'll check and refactor out this entire block and prevPhrases, newPhrases 
                            //data = String.Join(" ", newPhrase);

                            //lblStatus.Text = lblStatus.Text + "*";
                            //lblStatus.Invalidate();
                            break;
                        }

                        n--;
                    }
                }
                if (newPhrase.Length > 2)
                {
                    prevPhrase[0] = newPhrase[newPhrase.Length - 3];
                    prevPhrase[1] = newPhrase[newPhrase.Length - 2];
                    prevPhrase[2] = newPhrase[newPhrase.Length - 1];
                    prevLen = 3;
                }
                else if (newPhrase.Length > 1)
                {
                    prevPhrase[0] = newPhrase[newPhrase.Length - 2];
                    prevPhrase[1] = newPhrase[newPhrase.Length - 1];
                    prevLen = 2;
                }
                else if (newPhrase.Length > 0)
                {
                    prevPhrase[0] = newPhrase[newPhrase.Length - 1];
                    prevLen = 1;
                }
                else prevLen = 0;
                //lblStatus.Text = String.Join(" ", prevPhrase);
                // end 

                #endregion

                if (counter1 > 0) sessTextLen1 += data.Length;
                if (counter2 > 0) sessTextLen2 += data.Length;

                if (newParagraph) { noSpace = true; capitalize = true; }
                if (newLine) { noSpace = true; }

                while (data.IndexOf(";;np;;") >= 0)
                {
                    data = data.Replace(";;np;;", " " + _tables.Cmd_word + " ENTER " + _tables.Cmd_word + " ENTER");
                    newParagraph = true;
                    //showFlags("1");
                }
                while (data.IndexOf(";;nl;;") >= 0)
                {
                    data = data.Replace(";;nl;;", _tables.Cmd_word + " ENTER");
                    newLine = true;
                    //showFlags("2");
                }

                data = data.Trim();
                if (data.Length == 0) return;

                if (!(newParagraph || newLine))
                {
                    if (capitalize)
                    {
                        data = data.Substring(0, 1).ToUpperInvariant() + data.Substring(1);
                        capitalize = false;
                        //showFlags("3");
                    }
                    else
                    {
                        //data = data.Substring(0, 1).ToLowerInvariant() + data.Substring(1);
                    }
                }

                if (data.Length == 1)
                {
                    // DRAGAN: TODO: should we add noSpaceAfter here as well? I don't think so
                    if (CHAR_NO_SPACE.IndexOf(data) != -1) { noSpace = true; /* showFlags("4"); */ }
                    //if (CHAR_BEFORE_CAP.IndexOf(data) == 0) { noSpace = true; /* showFlags("4"); */ }
                }

                // DRAGAN: just some debug dump, it can stay as it's only used in/when debugging
                Debug.WriteLine($"sending data: {data}, '{DateTime.Now.ToString()}'");
                _sender.SendPhrase(data, ref noSpace);

                if (newParagraph)
                {
                    //noSpace = true; // DRAGAN: trying to fix backtracking on new line/paragraph
                    // NOTE: paragraph/line has 2-way nospace effect, first when we add new line we backspace previous space,
                    // and then when we write something after the new line, we backspace again?
                    capitalize = true;
                    newParagraph = false;
                    //showFlags("5");
                }
                else if (newLine)
                {
                    //noSpace = true; // DRAGAN: trying to fix backtracking on new line/paragraph
                    // NOTE: paragraph/line has 2-way nospace effect, first when we add new line we backspace previous space,
                    // and then when we write something after the new line, we backspace again?
                    newLine = false;
                    //showFlags("6");
                }
                else
                {
                    noSpace = false;
                    string lastChar = data.Substring(data.Length - 1);
                    if (CHAR_BEFORE_CAP.IndexOf(lastChar) >= 0)
                    {
                        capitalize = true;
                        //showFlags("7");
                    }
                }

            }
            else if (status == 1 && data.IndexOf("OK") >= 0)
            {
                status = 2;
                overlap = secCount + 3;

                // this is to raise the event for app to listen and minimize the window
                OnStarted();
                LocalSettings.ChangeIcon("Green");
                Status = RecordingStatus.Active;
                //lblStartStop.Image = Properties.Resources.mic_green;

                startSound.Play();
            }
        }

        #endregion

        #region Helper Methods

        // DRAGAN: we're not killing processes any more (except when we click the mic button to stop recording), so a new param
        // is introduced to distinguish and allow for 'soft kill' (i.e. doing nothing). That soft kill is mostly called on 
        // the 'rollover' i.e. each 45 seconds when previously we used to stop/restart and overlap 2 processes. That's no more
        private void KillSTT(Process stt, bool forReal = true)
        {
            try
            {
                if (forReal && !stt.HasExited)
                {
                    stt.CancelOutputRead();
                    stt.Kill();
                    while (!stt.HasExited) System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong. We apologize!" + "\n" + "Please restart the application and try again. If the issue persists and you are able to reproduce it, then please click 'Yes' to report your findings", 
                                                    "Blue-Maria", 
                                                    MessageBoxButton.YesNo, 
                                                    MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                }
                Debug.WriteLine($"{e.Message}");
               

            } // /* MessageBox.Show(e.Message+" (1)"); */ }
        }

        void ShowFlags(string p)
        {
            string np = newParagraph ? "np" : "";
            string nl = newLine ? " nl" : "";
            string ns = noSpace ? " ns" : "";
            string cap = capitalize ? " cap" : "";

            //lblStatus.Text = lblStatus.Text + "\n" + p + ":" + np + nl + ns + cap;
        }

        #endregion
    }
}
