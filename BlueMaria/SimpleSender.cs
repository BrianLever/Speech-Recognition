//using BlueMaria.Utilities;
//using System;
//using System.Collections;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text.RegularExpressions;
////using System.Windows.Forms;

//namespace BlueMaria
//{
//    public class SimpleSender : ISender
//    {
//        private readonly Action<string> _logText;
//        private readonly ITables _tables;
//        private readonly System.Windows.Forms.TextBox _targetTextBox;

//        public SimpleSender(Action<string> logText, ITables tables, System.Windows.Forms.TextBox targetTextBox)
//        {
//            _logText = logText;
//            _tables = tables;
//            _targetTextBox = targetTextBox;
//        }

//        public void SendPhrase(string s, ref bool noSpace)
//        {
//            var replaced = _tables.Replace(s);
//            if (replaced != s) { } // noSpace = true; // noSpaceAfter = true;
//            s = replaced;

//            // we're skipping any commands etc.

//            if (noSpace)
//            {
//                //_targetTextBox.AppendText(((char)08).ToString()); // send the backspace
//                if (_targetTextBox.TextLength > 0)
//                {
//                    _targetTextBox.Text = _targetTextBox.Text.Substring(0, _targetTextBox.Text.Length - 1);
//                    _targetTextBox.SelectionStart = _targetTextBox.Text.Length;
//                    _targetTextBox.ScrollToCaret();
//                }
//            }

//            _targetTextBox.AppendText(s + " ");
//            noSpace = false;
//        }

//    }
//}