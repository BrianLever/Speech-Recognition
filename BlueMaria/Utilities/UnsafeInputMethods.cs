using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows.Forms;

namespace BlueMaria.Utilities
{
	internal sealed class UnsafeInputMethods
	{
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern short GetKeyState(int nVirtKey);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern uint SendInput(uint nInputs, NativeMethods.INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char c);

        ////[DllImport("user32.dll", SetLastError = true)]
        ////public static extern int ToAscii(
        ////    uint uVirtKey,
        ////    uint uScanCode,
        ////    byte[] lpKeyState,
        ////    out uint lpChar,
        ////    uint flags
        ////    );

        //[DllImport("user32.dll")]
        //private static extern int ToAscii(uint uVirtKey, uint uScanCode,
        //                                  byte[] lpKeyState,
        //                                  [Out] StringBuilder lpChar,
        //                                  uint uFlags);

        //public static char ToAscii(Keys key, Keys modifiers) => ToAscii((uint)key, modifiers);
        //public static char ToAscii(uint key, Keys modifiers)
        //{
        //    var outputBuilder = new StringBuilder(2);
        //    int result = ToAscii((uint)key, 0, GetKeyState(modifiers),
        //                         outputBuilder, 0);
        //    if (result == 1)
        //        return outputBuilder[0];
        //    else
        //    {
        //        Debug.WriteLine($"invalid key:...");
        //        return (char)key;
        //        //throw new Exception("Invalid key");
        //    }


        //    //// set shift key pressed
        //    //byte[] b = new byte[256];
        //    //b[0x10] = 0x80;

        //    //uint r;
        //    //// return value of 1 expected (1 character copied to r)
        //    //if (1 != UnsafeInputMethods.ToAscii(code, code, b, out r, 0))
        //    //    return c; // throw new ApplicationException("Could not translate modified state");
        //    ////System.Windows.Forms.Keys.ShiftKey
        //    //return (char)r;

        //}

        //public static uint CharToCode(char c)
        //{
        //    //UnsafeInputMethods.ToAscii((uint)c, System.Windows.Forms.Keys.ShiftKey);
        //    short vkKeyScanResult = UnsafeInputMethods.VkKeyScan(c);

        //    // a result of -1 indicates no key translates to input character
        //    if (vkKeyScanResult == -1)
        //        return c; // throw new ArgumentException("No key mapping for " + c);

        //    // vkKeyScanResult & 0xff is the base key, without any modifiers
        //    uint code = (uint)vkKeyScanResult & 0xff;

        //    return code;
        //}

        //private const byte HighBit = 0x80;
        //private static byte[] GetKeyState(Keys modifiers)
        //{
        //    var keyState = new byte[256];
        //    foreach (Keys key in Enum.GetValues(typeof(Keys)))
        //    {
        //        if ((modifiers & key) == key)
        //        {
        //            keyState[(int)key] = HighBit;
        //        }
        //    }
        //    return keyState;
        //}

    }
}
