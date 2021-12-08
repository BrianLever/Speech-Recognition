using BlueMaria.Utilities;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueMaria
{
    public class LowLevelSender : ISender
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SPECIAL_SYMBOLS = "+^%~(){}";
        private const string NOSPACEALLOWED = "{NSA}";
        private const string CAPITALNEXT = "{CAP}";
        private const string ESCAPESYMBOL = "SYMBOL";
        private const string ESCAPECOMMAND = "COMMAND";

        private readonly Action<string> _logText;
        private readonly ITables _tables;
        bool nextCap = false;

        public LowLevelSender(Action<string> logText, ITables tables)
        {
            _logText = logText;
            _tables = tables;
        }

        public void PressKey(short k)
        {
            var inputs = new NativeMethods.INPUT[1];
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].inputUnion.ki.wVk = k;
            UnsafeInputMethods.SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
        }

        public void ReleaseKey(short k)
        {
            var inputs = new NativeMethods.INPUT[1];
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].inputUnion.ki.wVk = k;
            inputs[0].inputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;
            UnsafeInputMethods.SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
        }

        public void SendKey(short k)
        {
            if (k == 92) k = 220;
            if (k == 58) PressKey(16);
            // this is to allow for backspace as we have that now
            if (k != 32 && k != 220 && k != 08)
            {
                if (k < 48 || k > 90) return;
                if (k > 58 && k < 65) return;
            }

            PressKey(k);
            ReleaseKey(k);
            if (k == 58) ReleaseKey(16);
        }

        public void SendWord(string s)
        {
            if (s.Length < 1) return;

            _logText(" --> " + s);

            for (int i = 0; i < s.Length; i++)
            {
                // FIX: DRAGAN: check for unicode characters, at the moment I'm limitting to 2 chars
                if (i + 1 < s.Length && _tables.HasUnicodesInC2K)
                {
                    // we try to get the 2-char unicode (starting from this char as the 'first' in the surrogate pair)
                    string cUnicode = s.Substring(i, 2);
                    string keycodesUnicode = (string)_tables.Lang_c2k[cUnicode];
                    if (keycodesUnicode != null)
                    {
                        SendKeyCodes(keycodesUnicode);
                        i++; // increment one more for that 2-nd char in the pair
                        // TODO: we shouldn't do this, it's a bit 'dirty', but no better quick-fix idea at the moment
                        continue;
                    }
                }

                string c = s.Substring(i, 1);
                string keycodes = (string)_tables.Lang_c2k[c];

                // FIX: DRAGAN: this is a temp fix, each language should have all characters covered, I guess?
                // I got this error when I was testing for Serbian and pronounced the 'c`' char which wasn't in the c2k
                // (I just copied english version to the serbia c2k, that's why, so that shouldn't happen, though who's to say?)
                if (keycodes == null)
                {
                    Log.Error($"error: char missing in the .c2k, this shouldn't happen?...{c.ToString()}");
                    continue;
                }

                SendKeyCodes(keycodes);
            }
        }

        private void SendKeyCodes(string keycodes)
        {
            string[] codes = keycodes.Split(',');
            for (int j = 0; j < codes.Length; j++)
            {
                if (codes[j].Substring(0, 1) == "C") SendKeys.SendWait(codes[j].Substring(1));
                else
                {
                    short k = Convert.ToInt16(codes[j].Substring(1));
                    if (codes[j].Substring(0, 1) == "P") PressKey(k);
                    else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
                }
            }
        }

        public int SendCommand(string[] r)
        {
            int used = 0;
            string cmd = "";
            foreach (DictionaryEntry pair in _tables.Lang_cmd)
            {
                cmd = (string)pair.Key;
                string[] cmd_words = cmd.Split(' ');
                bool found = true;
                for (int j = 0; j < cmd_words.Length; j++)
                {
                    if (j >= r.Length) { found = false; break; }
                    //if(r[j].ToUpper() == cmd_word) { found = false; break; }
                    if (cmd_words[j] != r[j].ToUpper()) { found = false; break; }
                }
                if (found)
                {
                    used = cmd_words.Length;
                    break;
                }
            }
            if (used > 0)
            {
                _logText(" --> " + cmd.ToUpper());

                string ks = (string)_tables.Lang_cmd[cmd];
                string[] codes = ks.Split(',');
                for (int j = 0; j < codes.Length; j++)
                {
                    if (codes[j].Substring(0, 1) == "C")
                    {
                        string toSend = codes[j].Substring(1);
                        if (toSend == _tables.Cmd_word) toSend = toSend.ToLower();
                        SendKeys.SendWait(toSend);
                        if (toSend.Equals(ESCAPECOMMAND,StringComparison.InvariantCultureIgnoreCase)) SendKey(32); //Parsing RQ_01_001
                    }
                    else
                    {
                        short k = Convert.ToInt16(codes[j].Substring(1));
                        if (codes[j].Substring(0, 1) == "P") PressKey(k);
                        else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
                    }
                    System.Threading.Thread.Sleep(20);
                }
            }

            return used;
        }

        public int SendSymbol(string[] r, ref bool CapNext)
        {
            int used = 0;
            string sbl = "";
            foreach (DictionaryEntry pair in _tables.Lang_sbl)
            {
                sbl = (string)pair.Key;
                string[] sbl_words = sbl.Split(' ');
                bool found = true;
                for (int j = 0; j < sbl_words.Length; j++)
                {
                    if (j >= r.Length) { found = false; break; }
                    if (sbl_words[j] != r[j].ToUpper()) { found = false; break; }
                }
                if (found)
                {
                    used = sbl_words.Length;
                    break;
                }
            }
            if (used > 0)
            {
                string ks = (string)_tables.Lang_sbl[sbl];
                string[] codes = ks.Split(',');
                for (int j = 0; j < codes.Length; j++)
                {
                    if (codes[j].Substring(0, 1) == "C")
                    {
                        string str = codes[j].Substring(1);
                        if (str == _tables.Sbl_word) str = str.ToLower();
                        if (SPECIAL_SYMBOLS.IndexOf(str) >= 0) SendKeys.SendWait("{" + str + "}");
                        else SendKeys.SendWait(str);
                        if (str.Equals(ESCAPESYMBOL, StringComparison.InvariantCultureIgnoreCase)) SendKey(32);
                    }
                    else
                    {
                        short k = Convert.ToInt16(codes[j].Substring(1));
                        if (codes[j].Substring(0, 1) == "P") PressKey(k);
                        else if (codes[j].Substring(0, 1) == "R") ReleaseKey(k);
                    }
                    System.Threading.Thread.Sleep(20);
                }
            }

            return used;
        }

        public void SendPhrase(string s, ref bool noSpace)
        {            
            var replaced = _tables.Replace(s);
            if (replaced != s) { } // noSpace = true; // noSpaceAfter = true;
            s = replaced;

            //var original = s;
            //var builder = new StringBuilder(s);
            //foreach (var pair in _tables.Lang_fre) builder = builder.Replace((string)pair.Key, (string)pair.Value);

            ////string lastChar = s.Substring(s.Length - 1);
            ////if (CHAR_BEFORE_CAP.IndexOf(lastChar) >= 0) noSpace = true;

            //// DRAGAN: quick fix, replacements should be treated differently to other words, normal words have separators (space)
            //// replacements should be 'in place'
            //if (s.Equals(original) == false) { } // noSpace = true; // noSpaceAfter = true;

            // TODO: DRAGAN: this is wrong, word delimiters could be many, not just spaces
            string[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                int used = 0;
                if (words[i].ToUpper() == _tables.Cmd_word)
                {
                    if (i < words.Length - 1)
                    {
                        string[] rest = new string[words.Length - i - 1];
                        Array.Copy(words, i + 1, rest, 0, words.Length - i - 1);
                        used = SendCommand(rest);
                    }
                }
                else if (words[i].ToUpper() == _tables.Sbl_word)
                {
                    if (i < words.Length - 1)
                    {
                        string[] rest = new string[words.Length - i - 1];
                        Array.Copy(words, i + 1, rest, 0, words.Length - i - 1);
                        used = SendSymbol(rest, ref nextCap);
                    }
                }
                else
                {
                    // DRAGAN: changing from space-word => word-space
                    
                    if (noSpace) SendKey(08); // send the backspace
                    if (nextCap)
                    {
                        words[i] = words[i].Substring(0, 1).ToUpperInvariant() + words[i].Substring(1);
                        nextCap = false;
                    }
                    bool shouldCapped = CheckCAPAfterSymbol(ref words[i]);
                    if (shouldCapped) { nextCap = true; }

                    bool isNSA = CheckForNSAToken(ref words[i]);
                    SendWord(words[i]);
                    // NOTE: TODO: this is near impossible to solve (this way), as any future e.g. '.' will backspace on this
                    // if we don't add the space here
                    if (!isNSA) SendKey(32); // if (!noSpaceAfter) sendKey(32);
                     noSpace = false;

                    //if (!noSpace) sendKey(32);
                    //noSpace = false;

                    //sendWord(words[i]);
                }
                i += used;
            }
        }

        /// <summary>
        /// Parsing RQ_01_003
        /// This method checks for the Symbol in the input string, if present it checks for the 
        /// (CAP) token after symbol, if present it removes that from the input string and returns true
        /// indicating that the next character after symbol should be capitalized.
        /// </summary>
        /// <param name="v"> a reference to the word that contains the symbol and or not CAP/param>
        /// <returns>True if NSA present else False </returns>
        private bool CheckCAPAfterSymbol(ref string v)
        {
           
            int len = v.IndexOf(CAPITALNEXT);
            if (len < 0) return false;
            v = v.Substring(0, len);
            return true;           
        }

        /// <summary>
        /// Parsing RQ_01_002
        /// This method removes the (NSA) from the input word that is read as part of the replacement text
        /// and also indicates the caller that NSA is present or not to make a decision to put a space or not
        /// </summary>
        /// <param name="v"> a reference to the word that contains the command NSA</param>
        /// <returns>True if NSA present else False </returns>
        private bool CheckForNSAToken(ref string v)
        {
            if (v.Contains(NOSPACEALLOWED))
            {
                int len = v.IndexOf(NOSPACEALLOWED);
                if (len < 0) return false;
                v = v.Substring(0, len);
                return true;
            }
            return false;
        }
    }
}