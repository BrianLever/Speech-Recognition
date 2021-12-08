//using BlueMaria.Utilities;
//using System;
//using System.Collections;
//using System.Runtime.InteropServices;
//using System.Text.RegularExpressions;
////using System.Windows.Forms;

//namespace BlueMaria
//{
//    public class TextBoxSender : ISender
//    {
//        private const string SPECIAL_SYMBOLS = "+^%~(){}";

//        private readonly Action<string> _logText;
//        private readonly ITables _tables;
//        private readonly System.Windows.Forms.TextBox _targetTextBox;

//        public TextBoxSender(Action<string> logText, ITables tables, System.Windows.Forms.TextBox targetTextBox)
//        {
//            _logText = logText;
//            _tables = tables;
//            _targetTextBox = targetTextBox;
//        }

//        bool _isShift = false; //true; // this was 'on' cause of the letters which are upside down
//        public void PressKey(short k)
//        {
//            var str = char.ConvertFromUtf32(k);
//            var chr = str[0];
//            var code = (uint)k;

//            if (k == 08) // backspace special handling
//            {
//                if (_targetTextBox.TextLength > 0)
//                {
//                    _targetTextBox.Text = _targetTextBox.Text.Substring(0, _targetTextBox.Text.Length - 1);
//                    _targetTextBox.SelectionStart = _targetTextBox.Text.Length;
//                    _targetTextBox.ScrollToCaret();
//                }
//                return;
//            }

//            if (k == 16) { _isShift = !_isShift; return; } // { _isShift = true; return; }

//            // this is a quick fix, as we're not (per tables) sending \r\n but just \r
//            if (k == 13) { _targetTextBox.AppendText(Environment.NewLine); return; }

//            if (_isShift || char.IsLetter(chr))
//                _targetTextBox.AppendText(code.GetShiftedKey().ToString()); // chr.ToString().ReverseCase(); // 
//            else
//                _targetTextBox.AppendText(str); // chr.ToString());

//            //if (_isShift || char.IsLetter((char)k))
//            //    _targetTextBox.AppendText(((char)k).GetModifiedKey().ToString()); // ((char)k).ToString().ReverseCase(); // 
//            //else
//            //    _targetTextBox.AppendText(((char)k).ToString());
//        }

//        public void ReleaseKey(short k)
//        {
//            if (k == 16) { _isShift = !_isShift; return; } // { _isShift = false; return; }
//        }

//        public void SendKey(short k)
//        {
//            if (k == 92) k = 220;
//            if (k == 58)
//            {
//                PressKey(16);
//            }
//            // this is to allow for backspace as we have that now
//            if (k != 32 && k != 220 && k != 08)
//            {
//                if (k < 48 || k > 90) return;
//                if (k > 58 && k < 65) return;
//            }

//            PressKey(k);
//            ReleaseKey(k);

//            if (k == 58) ReleaseKey(16);
//        }

//        public void SendWord(string s)
//        {
//            if (s.Length < 1) return;

//            _logText(" --> " + s);

//            for (int i = 0; i < s.Length; i++)
//            {
//                string c = s.Substring(i, 1);
//                string keycodes = (string)_tables.Lang_c2k[c];

//                string[] codes = keycodes.Split(',');
//                for (int j = 0; j < codes.Length; j++)
//                {
//                    if (codes[j].Substring(0, 1) == "C")
//                    {
//                        _targetTextBox.AppendText(codes[j].Substring(1));
//                    }
//                    else
//                    {
//                        short k = Convert.ToInt16(codes[j].Substring(1));
//                        if (codes[j].Substring(0, 1) == "P") PressKey(k);
//                        else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
//                    }
//                }
//            }
//        }

//        public int SendCommand(string[] r)
//        {
//            int used = 0;
//            string cmd = "";
//            foreach (DictionaryEntry pair in _tables.Lang_cmd)
//            {
//                cmd = (string)pair.Key;
//                string[] cmd_words = cmd.Split(' ');
//                bool found = true;
//                for (int j = 0; j < cmd_words.Length; j++)
//                {
//                    if (j >= r.Length) { found = false; break; }
//                    //if(r[j].ToUpper() == cmd_word) { found = false; break; }
//                    if (cmd_words[j] != r[j].ToUpper()) { found = false; break; }
//                }
//                if (found)
//                {
//                    used = cmd_words.Length;
//                    break;
//                }
//            }
//            if (used > 0)
//            {
//                _logText(" --> " + cmd.ToUpper());

//                string ks = (string)_tables.Lang_cmd[cmd];
//                string[] codes = ks.Split(',');
//                for (int j = 0; j < codes.Length; j++)
//                {
//                    if (codes[j].Substring(0, 1) == "C")
//                    {
//                        string toSend = codes[j].Substring(1);
//                        if (toSend == _tables.Cmd_word) toSend = toSend.ToLower();
//                        _targetTextBox.AppendText(toSend);
//                    }
//                    else
//                    {
//                        short k = Convert.ToInt16(codes[j].Substring(1));
//                        if (codes[j].Substring(0, 1) == "P") PressKey(k);
//                        else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
//                    }
//                    System.Threading.Thread.Sleep(20);
//                }
//            }

//            return used;
//        }

//        public int SendSymbol(string[] r)
//        {
//            int used = 0;
//            string sbl = "";
//            foreach (DictionaryEntry pair in _tables.Lang_sbl)
//            {
//                sbl = (string)pair.Key;
//                string[] sbl_words = sbl.Split(' ');
//                bool found = true;
//                for (int j = 0; j < sbl_words.Length; j++)
//                {
//                    if (j >= r.Length) { found = false; break; }
//                    if (sbl_words[j] != r[j].ToUpper()) { found = false; break; }
//                }
//                if (found)
//                {
//                    used = sbl_words.Length;
//                    break;
//                }
//            }
//            if (used > 0)
//            {
//                string ks = (string)_tables.Lang_sbl[sbl];
//                string[] codes = ks.Split(',');
//                for (int j = 0; j < codes.Length; j++)
//                {
//                    if (codes[j].Substring(0, 1) == "C")
//                    {
//                        string str = codes[j].Substring(1);
//                        if (str == _tables.Sbl_word) str = str.ToLower();
//                        if (SPECIAL_SYMBOLS.IndexOf(str) >= 0)
//                        {
//                            _targetTextBox.AppendText("{" + str + "}");
//                        }
//                        else
//                        {
//                            _targetTextBox.AppendText(str);
//                        }
//                    }
//                    else
//                    {
//                        short k = Convert.ToInt16(codes[j].Substring(1));
//                        if (codes[j].Substring(0, 1) == "P") PressKey(k);
//                        else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
//                    }
//                    System.Threading.Thread.Sleep(20);
//                }
//            }

//            return used;
//        }

//        public void SendPhrase(string s, ref bool noSpace)
//        {
//            var replaced = _tables.Replace(s);
//            if (replaced != s) { } // noSpace = true; // noSpaceAfter = true;
//            s = replaced;

//            //// DRAGAN: quick fix, replacements should be treated differently to other words, normal words have separators (space)
//            //// replacements should be 'in place'
//            //if (s.Equals(original) == false) { } // noSpace = true; // noSpaceAfter = true;

//            // TODO: DRAGAN: this is wrong, word delimiters could be many, not just spaces
//            string[] words = s.Split(' ');
//            for (int i = 0; i < words.Length; i++)
//            {
//                if (words[i].Length == 0) continue;

//                // TODO: rework this better, but we need a proper parser
//                //var words = s.Split();
//                //if (words[0] == _tables.Cmd_word)
//                //{
//                //    string cmd = string.Join(" ", words.Skip(1));
//                //    var value = _tables.Lang_cmd[cmd];
//                //    if (value != null)
//                //    {
//                //        _logText(" --> " + cmd.ToUpper());
//                //    }
//                //}

//                int used = 0;
//                if (words[i].ToUpper() == _tables.Cmd_word)
//                {
//                    if (i < words.Length - 1)
//                    {
//                        string[] rest = new string[words.Length - i - 1];
//                        Array.Copy(words, i + 1, rest, 0, words.Length - i - 1);
//                        used = SendCommand(rest);
//                    }
//                }
//                else if (words[i].ToUpper() == _tables.Sbl_word)
//                {
//                    if (i < words.Length - 1)
//                    {
//                        string[] rest = new string[words.Length - i - 1];
//                        Array.Copy(words, i + 1, rest, 0, words.Length - i - 1);
//                        used = SendSymbol(rest);
//                    }
//                }
//                else
//                {
//                    // DRAGAN: changing from space-word => word-space

//                    if (noSpace) SendKey(08); // send the backspace

//                    SendWord(words[i]);
//                    // NOTE: TODO: this is near impossible to solve (this way), as any future e.g. '.' will backspace on this
//                    // if we don't add the space here
//                    SendKey(32); // if (!noSpaceAfter) sendKey(32);
//                    noSpace = false;

//                    //if (!noSpace) sendKey(32);
//                    //noSpace = false;

//                    //sendWord(words[i]);
//                }
//                i += used;
//            }
//        }

//    }
//}