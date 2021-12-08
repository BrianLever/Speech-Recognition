using BlueMaria.StaticFunction;
using BlueMaria.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows;

namespace BlueMaria
{
    public class Tables : ITables
    {
        private const string BaseC2KExt = "c2k.zip"; // "c2k.txt"; // "map.txt"; // 
        private const string C2KExt = "c2k.txt";

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Regex _wordBoundaries; // = new Regex(@"\btest\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //private List<KeyTable> _keytables = new List<KeyTable>();
        //private List<KeyTable> _cmdtables = new List<KeyTable>();
        //private List<KeyTable> _sbltables = new List<KeyTable>();
        //private List<KeyTable> _fretables = new List<KeyTable>();

        private KeyTable _keytable = null;
        private KeyTable _cmdtable = null;
        private KeyTable _sbltable = null;
        private KeyTable _fretable = null;

        private List<string> _languages { get; set; } = new List<string>();
        private List<string> _languageNames { get; set; } = new List<string>();

        public string Cmd_word { get; set; } = "";
        public string Sbl_word { get; set; } = "";

        public Hashtable Lang_c2k { get; set; }
        public bool HasUnicodesInC2K { get; set; }
        public Hashtable Lang_cmd { get; set; }
        public Hashtable Lang_sbl { get; set; }
        public Dictionary<string, string> Lang_fre { get; set; }

        public IEnumerable<string> Languages => _languages; //.ToArray();
        public IEnumerable<string> LanguageNames => _languageNames; //.ToArray();

        public Dictionary<string, string> LanguagesHash { get; set; } = new Dictionary<string, string>();

        public string LoadLanguages()
        {
            //cbLanguages.Items.Clear();
            _languageNames.Clear();
            _languages.Clear();
            LanguagesHash = new Dictionary<string, string>();

            if (!File.Exists("languages.def")) return "Can’t start because the 'languages.def' file is missing";

            string[] lines = File.ReadAllLines("languages.def");
            string errormsg = "";
            foreach (string line in lines)
            {
                string li = line.Trim();
                if (li.Contains('#')) li = li.Substring(0, li.IndexOf('#'));
                if (li.Length < 3) continue;

                string[] fields = li.Split('=');

                string description = fields[0];
                string name = fields[1];

                var nameParts = name.Split('_');
                string language = nameParts[0];
                string keyboard = (nameParts.Length > 1) ? nameParts[1] : "";
                var keyboardLanguageName = $"{keyboard}_{language}";

                if (!File.Exists("keytables\\base.c2k") && !File.Exists($"keytables\\base.{BaseC2KExt}"))
                {
                    errormsg = $"Can’t start because  base.{BaseC2KExt} is missing"; return errormsg;
                }

                if (!string.IsNullOrEmpty(keyboard) && 
                !File.Exists($"keytables\\{keyboard}.c2k") && !File.Exists($"keytables\\{keyboard}.{C2KExt}"))
                {
                   //  errormsg = $"Can’t start because {keyboard}.c2k is missing"; return errormsg;
                    continue;
                    //Log.Error($"'{keyboard}.c2k', no keyboard c2k file present: {li}");                 
                   // continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_commands.def")) // name
                {

                   //errormsg = $"Can’t start because {keyboardLanguageName}_commands.def is missing"; return errormsg;
                    continue;
                    //Log.Error($"'{name}_commands.def', no commands file present: {li}");
                    //  LocalSettings.StartupError?.Invoke(this, EventArgs.Empty);
                    //continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_symbols.def")) // name
                {
                   // errormsg = $"Can’t start because  {keyboardLanguageName}_symbols.def is missing"; return errormsg;
                    continue;
                    // Log.Error($"'{name}_symbols.def', no symbols file present: {li}");
                    //  errormsg = $"'{name}_symbols.def', no symbols file present: {li}"; return errormsg;
                    //LocalSettings.StartupError?.Invoke(this, EventArgs.Empty);
                    //continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_replace.def")) // name
                {
                    continue;

                    // continue;
                    //Log.Error($"'{name}_replace.def', no replace file present: {name}");
                     // errormsg = $"Can’t start because  {keyboardLanguageName}_replace.def is missing"; return errormsg;
                    //return errormsg;
                    //return errormsg;
                }


                //if (File.Exists("keytables\\" + name + ".c2k"))
                {
                    //cbLanguages.Items.Add(description);
                    _languageNames.Add(description);
                    _languages.Add(name);

                    if (LanguagesHash.ContainsKey(name)) { } // duplicate key?
                    if (string.IsNullOrWhiteSpace(name)) { } //
                    LanguagesHash[name] = description;

                    //errormsg = LoadC2K(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadCommands(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadSymbols(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadReplacements(name);
                    //if (errormsg.Length > 0) break;
                }
            }
            return errormsg;

            //cbLanguages.SelectedIndex = 0;
        }

        public string checkfiles()
        {
            _languageNames.Clear();
            _languages.Clear();
            LanguagesHash = new Dictionary<string, string>();

            if (!File.Exists("languages.def")) return "Can’t start because the 'languages.def' file is missing";

            string[] lines = File.ReadAllLines("languages.def");
            string errormsg = "";
            foreach (string line in lines)
            {
                string li = line.Trim();
                if (li.Contains('#')) li = li.Substring(0, li.IndexOf('#'));
                if (li.Length < 3) continue;

                string[] fields = li.Split('=');

                string description = fields[0];
                string name = fields[1];

                var nameParts = name.Split('_');
                string language = nameParts[0];
                string keyboard = (nameParts.Length > 1) ? nameParts[1] : "";
                var keyboardLanguageName = $"{keyboard}_{language}";

                if (!File.Exists("keytables\\base.c2k") && !File.Exists($"keytables\\base.{BaseC2KExt}"))
                {
                    errormsg = $"Can’t start because  base.{BaseC2KExt} is missing"; return errormsg;
                }

                if (!string.IsNullOrEmpty(keyboard) &&
                !File.Exists($"keytables\\{keyboard}.c2k") && !File.Exists($"keytables\\{keyboard}.{C2KExt}"))
                {

                    errormsg = $"Can’t start because  {keyboard}.c2k is missing"; return errormsg;
                    //Log.Error($"'{keyboard}.c2k', no keyboard c2k file present: {li}");                 
                  //  continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_commands.def")) // name
                {
                    //                  System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    //                  {
                    //                      System.Windows.MessageBox.Show($"Can’t start because the configuration file {name}_commands.def is missing " + "\n" + "\n" + "Please correct the error or get more information about configuration files on our online help page.",
                    //"Blue-Maria", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    //                  }
                    //                   ));
                     errormsg = $"Can’t start because  {name}_commands.def is missing"; return errormsg;
                    //Log.Error($"'{name}_commands.def', no commands file present: {li}");
                    //  LocalSettings.StartupError?.Invoke(this, EventArgs.Empty);
                    //continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_symbols.def")) // name
                {
                    errormsg = $"Can’t start because  {name}_symbols.def is missing"; return errormsg;
                    //                  System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    //                  {
                    //                      System.Windows.MessageBox.Show($"Can’t start because the configuration file {name}_symbols.def is missing " + "\n" + "\n" + "Please correct the error or get more information about configuration files on our online help page.",
                    //"Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);
                    //                  }
                    //                 ));
                    //Log.Error($"'{name}_symbols.def', no symbols file present: {li}");
                    //LocalSettings.StartupError?.Invoke(this, EventArgs.Empty);
                  //  continue;
                }

                if (!File.Exists($"keytables\\{keyboardLanguageName}_replace.def")) // name
                {
                    errormsg = $"Can’t start because  {name}_replace.def is missing"; return errormsg;
                    //                  System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    //                  {
                    //                      System.Windows.MessageBox.Show($"Can’t start because the configuration file {name}_replace.def is missing " + "\n" + "\n" + "Please correct the error or get more information about configuration files on our online help page.",
                    //"Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);
                    //                  }
                    //                ));

                  //  continue;
                    //Log.Error($"'{name}_replace.def', no replace file present: {name}");
                    //errormsg = $"'{name}_replace.def', no replace file present: {name}";
                    //return errormsg;
                }


                //if (File.Exists("keytables\\" + name + ".c2k"))
                {
                    //cbLanguages.Items.Add(description);
                    _languageNames.Add(description);
                    _languages.Add(name);

                    if (LanguagesHash.ContainsKey(name)) { } // duplicate key?
                    if (string.IsNullOrWhiteSpace(name)) { } //
                    LanguagesHash[name] = description;

                    //errormsg = LoadC2K(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadCommands(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadSymbols(name);
                    //if (errormsg.Length > 0) break;

                    //errormsg = LoadReplacements(name);
                    //if (errormsg.Length > 0) break;
                }
            }
            return errormsg;

        }

        private string LoadReplacements(string name) //, string keyboardLanguageName)
        {
            // DRAGAN: a quick fix only, we don't seem to need both
            //name = keyboardLanguageName;

            string errormsg = "";

            if (!File.Exists($"keytables\\{name}_replace.def"))
            {
                Log.Error($"'{name}_replace.def', no replace file present: {name}");
                //errormsg = $"'{name}_replace.def', no replace file present: {name}";
                return errormsg;
            }

            if (File.Exists($"keytables\\{name}_replace.def"))
            {
                // Load free word(s) replace table
                Hashtable ht1 = new Hashtable();
                string[] ft = File.ReadAllLines("keytables\\" + name + "_replace.def");
                for (int i = 0; i < ft.Length; i++)
                {

                    try
                    {
                        if (ft[i].Length < 3) continue;

                        if (ft[i].IndexOf('=') < 0)
                        {
                            errormsg = "Can’t start because the configuration file \\" + name + "_replace.def has an error at line " + (i + 1).ToString();
                            break;
                        }

                        string[] ff = ft[i].Split('=');
                        int n = ft[i].IndexOf('=');         // To allow char '=' at the right part of the = separator
                        ff[0] = ft[i].Substring(0, n);
                        ff[1] = ft[i].Substring(n + 1);
                        ht1.Add(ff[0], ff[1]);
                    }
                    catch (Exception ex)
                    {
                        errormsg = "Can’t start because the configuration file \\" + name + "_replace.def has an error at line " + (i + 1).ToString();
                        return errormsg;
                    }
                }
                if (errormsg.Length > 0) return errormsg;
                _fretable = new KeyTable(name, ht1);
            }

            return errormsg;
        }

        private string LoadSymbols(string name) //, string keyboardLanguageName)
        {
            string errormsg = "";

            if (!File.Exists($"keytables\\{name}_symbols.def"))
            {
                Log.Error($"'{name}_symbols.def', no symbols file present: {name}");
                errormsg = $"'{name}_symbols.def', no symbols file present: {name}";
                return errormsg;
            }

            if (File.Exists($"keytables\\{name}_symbols.def"))
            {
                // Load symbol to keycodes translation table
                Hashtable ht1 = new Hashtable();
                string[] st = File.ReadAllLines("keytables\\" + name + "_symbols.def");
                for (int i = 0; i < st.Length; i++)
                {
                    try
                    {
                        if (st[i].Length < 3) continue;

                        if (st[i].IndexOf('=') < 0)
                        {
                            errormsg = "Can’t start because the configuration file\\" + name + "_symbols.def has an error at line " + (i + 1).ToString();
                            break;
                        }

                        string[] sf = new string[2];
                        int n = st[i].IndexOf('=');         // To allow entries like EQUALS==
                        sf[0] = st[i].Substring(0, n);
                        sf[1] = st[i].Substring(n + 1);
                        ht1.Add(sf[0], sf[1]);
                    }
                    catch (Exception ex)
                    {
                        errormsg = "Can’t start because the configuration file\\" + name + "_symbols.def has an error at line " + (i + 1).ToString();
                        return errormsg;
                    }
                }
                if (errormsg.Length > 0) return errormsg;
                _sbltable = new KeyTable(name, ht1);
            }

            return errormsg;
        }

        private string LoadCommands(string name) //, string keyboardLanguageName)
        {
            string errormsg = "";

            if (!File.Exists($"keytables\\{name}_commands.def"))
            {
                Log.Error($"'{name}_commands.def', no commands file present: {name}");
                errormsg = $"'{name}_commands.def', no commands file present: {name}";
                return errormsg;
            }

            if (File.Exists($"keytables\\{name}_commands.def"))
            {
                // Load command to keycodes translation table
                Hashtable ht1 = new Hashtable();
                string[] ct = File.ReadAllLines("keytables\\" + name + "_commands.def");
                for (int i = 0; i < ct.Length; i++)
                {
                    try
                    {
                        if (ct[i].Length < 3) continue;

                        if (ct[i].IndexOf('=') < 0)
                        {
                            errormsg = "Can’t start because the configuration file \\" + name + "_commands.def has an error at line " + (i + 1).ToString();
                            break;
                        }

                        string[] cf = ct[i].Split('=');
                        ht1.Add(cf[0], cf[1]);
                    }
                    catch (Exception ex)
                    {
                        errormsg = "Can’t start because the configuration file \\" + name + "_commands.def has an error at line " + (i + 1).ToString();
                        return errormsg;
                    }
                }
                if (errormsg.Length > 0) return errormsg;
                _cmdtable = new KeyTable(name, ht1);
            }

            return errormsg;
        }

        private string LoadC2K(string name)
        {
            string errormsg = "";
            // Load char to keycodes translation table
            Hashtable hash = new Hashtable();
            var hasUnicodes = false;

            var nameParts = name.Split('_');
            string language = nameParts[0];
            string keyboard = (nameParts.Length > 1) ? nameParts[1] : "";

            if (!File.Exists("keytables\\base.c2k") && !File.Exists($"keytables\\base.{BaseC2KExt}"))
            {
                errormsg = $"no base.{BaseC2KExt}?"; return errormsg;
            }

            if (!string.IsNullOrEmpty(keyboard) &&
                !File.Exists($"keytables\\{keyboard}.c2k") && !File.Exists($"keytables\\{keyboard}.{C2KExt}"))
            {
                Log.Error($"'{keyboard}.{C2KExt}', no keyboard c2k file present: {name}");
                errormsg = $"'{keyboard}.{C2KExt}', no keyboard c2k file present: {name}";
                return errormsg;
            }

            // $"{name}.c2k"
            string baseC2kFileName = File.Exists("keytables\\base.c2k") ? $"base.c2k" : $"base.{BaseC2KExt}";
            errormsg = ReadBaseC2K(baseC2kFileName, hash, ref hasUnicodes);
            if (errormsg.Length > 0) return errormsg;

            if (!string.IsNullOrEmpty(keyboard))
            {
                string c2kFileName = File.Exists($"keytables\\{keyboard}.c2k") ? $"{keyboard}.c2k" : $"{keyboard}.{C2KExt}";
                errormsg = ReadC2K(c2kFileName, hash, ref hasUnicodes);
                if (errormsg.Length > 0) return errormsg;
            }

            _keytable = new KeyTable(name, hash, hasUnicodes);

            return errormsg;
        }

        private static string ReadC2K(string c2kFileName, Hashtable hash, ref bool hasUnicodes)
        {
            string errormsg = "";

            string[] kt = File.ReadAllLines($"keytables\\{c2kFileName}");
            for (int i = 0; i < kt.Length; i++)
            {
                if (kt[i].Length < 3) continue;

                if (kt[i].Substring(1, 1) != "=")
                {
                    hasUnicodes = true;
                    // NOTE: DRAGAN: this is to fix issue with issue with Unicode chars > 2^16 (OxFFFF).
                    // What happens is that single C# char is 2 bytes/16 bits (2^16). After that it's
                    // 'split' into more chars. Original code was exepcting single char, i.e. = at the
                    // position '1'
                    var parts = kt[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        errormsg = $"No equal sign (or too many)? Syntax error in keytables\\{c2kFileName} file at line { (i + 1) }";
                        return errormsg;
                    }
                    // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
                    hash[parts[0]] = parts[1];
                    //hash.Add(parts[0], parts[1]);
                    continue;
                }

                // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
                string key = kt[i].Substring(0, 1);
                string value = kt[i].Substring(2);
                hash[key] = value;
                //hash.Add(key, value);
            }

            return errormsg;
        }

        private static string ReadBaseC2K(string c2kFileName, Hashtable hash, ref bool hasUnicodes)
        {
            string errormsg = "";

            using (var file = File.OpenRead($"keytables\\{c2kFileName}"))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count != 1)
                {
                    errormsg = $"zip should have only one file, keytables\\{c2kFileName} file";
                    return errormsg;
                }

                var entry = zip.Entries.First();

                using (var stream = entry.Open())
                {
                    string[] kt = stream.ReadLines().ToArray();
                    for (int i = 0; i < kt.Length; i++)
                    {
                        if (kt[i].Length < 3) continue;

                        if (kt[i].Substring(1, 1) != "=")
                        {
                            hasUnicodes = true;
                            // NOTE: DRAGAN: this is to fix issue with issue with Unicode chars > 2^16 (OxFFFF).
                            // What happens is that single C# char is 2 bytes/16 bits (2^16). After that it's
                            // 'split' into more chars. Original code was exepcting single char, i.e. = at the
                            // position '1'
                            var parts = kt[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length != 2)
                            {
                                errormsg = $"No equal sign (or too many)? Syntax error in keytables\\{c2kFileName} file at line { (i + 1) }";
                                return errormsg;
                            }
                            // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
                            hash[parts[0]] = parts[1];
                            //hash.Add(parts[0], parts[1]);
                            continue;
                        }

                        // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
                        string key = kt[i].Substring(0, 1);
                        string value = kt[i].Substring(2);
                        hash[key] = value;
                        //hash.Add(key, value);
                    }

                    return errormsg;

                }


                //using (StreamReader sr = new StreamReader(entry.Open()))
                //{
                //    Console.WriteLine(sr.ReadToEnd());
                //}
                //foreach (var entry in zip.Entries)
                //{
                //    using (var stream = entry.Open())
                //    {
                //    }
                //}
            }


            //string[] kt = File.ReadAllLines($"keytables\\{c2kFileName}");
            //for (int i = 0; i < kt.Length; i++)
            //{
            //    if (kt[i].Length < 3) continue;

            //    if (kt[i].Substring(1, 1) != "=")
            //    {
            //        hasUnicodes = true;
            //        // NOTE: DRAGAN: this is to fix issue with issue with Unicode chars > 2^16 (OxFFFF).
            //        // What happens is that single C# char is 2 bytes/16 bits (2^16). After that it's
            //        // 'split' into more chars. Original code was exepcting single char, i.e. = at the
            //        // position '1'
            //        var parts = kt[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            //        if (parts.Length != 2)
            //        {
            //            errormsg = $"No equal sign (or too many)? Syntax error in keytables\\{c2kFileName} file at line { (i + 1) }";
            //            return errormsg;
            //        }
            //        // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
            //        hash[parts[0]] = parts[1];
            //        //hash.Add(parts[0], parts[1]);
            //        continue;
            //    }

            //    // to allow for replacing base.c2k with the keyboard.c2k, we're not just adding
            //    string key = kt[i].Substring(0, 1);
            //    string value = kt[i].Substring(2);
            //    hash[key] = value;
            //    //hash.Add(key, value);
            //}

            //return errormsg;
        }

        public void ClearTables()
        {
            Cmd_word = "";
            //Sbl_word = "";
            Lang_c2k = null;
            HasUnicodesInC2K = false;
            Lang_cmd = null;
            Lang_sbl = null;
            Lang_fre = new Dictionary<string, string>(); // null;

            _keytable = null;
            _cmdtable = null;
            _sbltable = null;
            _fretable = null;
        }

        public string SetupTables(string name)
        {
            string value;
            var nameParts = name.Split('_');
            string language = nameParts[0];
            string keyboard = (nameParts.Length > 1) ? nameParts[1] : "";
            var keyboardLanguageName = $"{keyboard}_{language}";

            string errormsg = "";

            if (!LanguagesHash.TryGetValue(name, out  value))
            {
                Log.Error($"{nameof(SetupTables)}: language not found??");
                return errormsg;
            }

            _keytable = null;
            _cmdtable = null;
            _sbltable = null;
            _fretable = null;

            errormsg = LoadC2K(name);
            if (errormsg.Length > 0) return errormsg;

            errormsg = LoadCommands(keyboardLanguageName); // name
            if (errormsg.Length > 0) return errormsg;

            errormsg = LoadSymbols(keyboardLanguageName); // name
            if (errormsg.Length > 0) return errormsg;

            errormsg = LoadReplacements(keyboardLanguageName); // name
            if (errormsg.Length > 0) return errormsg;

            if (_keytable != null)
            {
                if (!(_keytable.Name == name))
                {
                    Log.Error($"_keytable should be for this language??");
                    errormsg = "_keytable should be for this language??";
                    return errormsg;
                }
                Lang_c2k = _keytable.Table;
                HasUnicodesInC2K = _keytable.HasUnicodes;
            }

            if (_cmdtable != null)
            {
                if (!(_cmdtable.Name == keyboardLanguageName)) //name))
                {
                    Log.Error($"_cmdtable should be for this language??");
                    errormsg = "_cmdtable should be for this language??";
                    return errormsg;
                }
                Lang_cmd = _cmdtable.Table;
            }

            if (_sbltable != null)
            {
                if (!(_sbltable.Name == keyboardLanguageName)) //name))
                {
                    Log.Error($"_sbltable should be for this language??");
                    errormsg = "_sbltable should be for this language??";
                    return errormsg;
                }
                Lang_sbl = _sbltable.Table;
            }

            if (_fretable != null)
            {
                if (!(_fretable.Name == keyboardLanguageName)) //name))
                {
                    Log.Error($"_fretable should be for this language??");
                    errormsg = "_fretable should be for this language??";
                    return errormsg;
                }

                Lang_fre = _fretable?.Dictionary;
                Lang_fre = Lang_fre ?? new Dictionary<string, string>();

                _wordBoundaries = new Regex(@"\b(" + string.Join("|", Lang_fre.Keys.Select(k => Regex.Escape(k.ToLower()))) + @")\b",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            if (Lang_cmd == null || Lang_sbl == null|| Lang_cmd.Count== 0||Lang_sbl.Count == 0)
            {
                Log.Error($"No cmd/sbl definition files are present?");
                errormsg = "No cmd/sbl definition files are present?";
                return errormsg;
            }
   
           
                Cmd_word = ((string)this.Lang_cmd["COMMAND"]).Substring(1);
                Sbl_word = ((string)this.Lang_sbl["SYMBOL"]).Substring(1);
            
           
            return errormsg;

            //for (int i = 0; i < _keytables.Count; i++)
            //{
            //    if (_keytables[i].Name == language)
            //    {
            //        Lang_c2k = _keytables[i].Table;
            //        HasUnicodesInC2K = _keytables[i].HasUnicodes;
            //        break;
            //    }
            //}
            //for (int i = 0; i < _cmdtables.Count; i++)
            //{
            //    if (_cmdtables[i].Name == language)
            //    {
            //        Lang_cmd = _cmdtables[i].Table;
            //        break;
            //    }
            //}
            //for (int i = 0; i < _sbltables.Count; i++)
            //{
            //    if (_sbltables[i].Name == language)
            //    {
            //        Lang_sbl = _sbltables[i].Table;
            //        break;
            //    }
            //}
            //for (int i = 0; i < _fretables.Count; i++)
            //{
            //    if (_fretables[i].Name == language)
            //    {
            //        Lang_fre = _fretables[i]?.Dictionary; //.table;
            //        Lang_fre = Lang_fre ?? new Dictionary<string, string>();

            //        _wordBoundaries = new Regex(@"\b(" + string.Join("|", Lang_fre.Keys.Select(k => Regex.Escape(k.ToLower()))) + @")\b",
            //            RegexOptions.IgnoreCase | RegexOptions.Compiled);

            //        break;
            //    }
            //}

            //Cmd_word = ((string)this.Lang_cmd["COMMAND"]).Substring(1);
            //Sbl_word = ((string)this.Lang_sbl["SYMBOL"]).Substring(1);
        }

        /// <summary>
        /// Do all replacements on a text based on the .def table of replacements
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ReplaceOld(string input)
        {
            string value;
            if (_wordBoundaries == null) return input;
            return _wordBoundaries.Replace(input, m =>
            {
                if (m?.Value == null) return m.Value; // superfluous but just in case
                if (this.Lang_fre.TryGetValue(m.Value.ToLower(), out  value))
                    return value;
                return m.Value;
                //return this.Lang_fre[m.Value.ToLower()];
            });
        }

        /// <summary>
        /// Do all replacements on a text based on the .def table of replacements
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Replace(string input)
        {
            //foreach (DictionaryEntry pair in lang_fre) s = s.Replace((string)pair.Key, (string)pair.Value);

            return input.ReplaceWords(this.Lang_fre);

            //return string.Join("", input.StringAndReplaceWords(this.Lang_fre));
            //var text = input;
            //var inputWords = text.SplitWords().Select(x => x.ToLower()).Distinct().ToDictionary(x => x);
            //foreach (var pair in Lang_fre)
            //{
            //    var hasAllWords = pair.Key.SplitWords().All(x => inputWords.ContainsKey(x.ToLower()));
            //    if (!hasAllWords) continue;
            //    var parts = text.Split(new[] { pair.Key }, StringSplitOptions.None);
            //    if (parts.Length <= 1) continue;
            //    StringBuilder s = new StringBuilder();
            //    for (int i = 0; i < parts.Length; ++i)
            //    {
            //        s.Append(parts[i]);
            //        if (!(i + 1 < parts.Length))
            //        {
            //            break;
            //        }
            //        if ((parts[i].Length > 0 && !parts[i].Last().IsAnySeparator()) ||
            //            (parts[i+1].Length > 0 && !parts[i+1].First().IsAnySeparator()))
            //        {
            //            s.Append(pair.Key);
            //            continue;
            //        }
            //        s.Append(pair.Value);
            //    }
            //    text = s.ToString();
            //}
            //return text;

            //Debug.WriteLine($"\n");
            //Debug.WriteLine($"input: {input}");
            //string replaced = string.Join("", input.SplitWordsAndReplace(this.Lang_fre));
            //Debug.WriteLine($"output: {replaced}\n\n");
            //return replaced;
        }

    }
}
