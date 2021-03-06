using BlueMaria.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BlueMaria
{
	public static class KeyboardInput
	{
		public static void SendKey(int key) {
			SendKey(null, key);
		}

		public static void SendKey(IEnumerable<int> modifierKeys, int key) {
			if (key <= 0) return;

			// Only a single key is pressed.
			if (modifierKeys == null || modifierKeys.Count() == 0) {
				var inputs = new NativeMethods.INPUT[1];
				inputs[0].type = NativeMethods.INPUT_KEYBOARD;
				inputs[0].inputUnion.ki.wVk = (short)key;
				UnsafeInputMethods.SendInput(1, inputs, Marshal.SizeOf(inputs[0]));
			} else {
				// A key with modifier keys is pressed.

				// To simulate this scenario, the inputs contains the toggling 
				// modifier keys, pressing the key and releasing modifier keys events.
				//
				// For example, to simulate Ctrl+C, we have to send 3 inputs:
				// 1. Ctrl is pressed.
				// 2. C is pressed. 
				// 3. Ctrl is released.
				var inputs = new NativeMethods.INPUT[modifierKeys.Count() * 2 + 1];

				int i = 0;

				// Simulate toggling the modifier keys.
				foreach (var modifierKey in modifierKeys) {
					inputs[i].type = NativeMethods.INPUT_KEYBOARD;
					inputs[i].inputUnion.ki.wVk = (short)modifierKey;
					i++;
				}

				// Simulate pressing the key.
				inputs[i].type = NativeMethods.INPUT_KEYBOARD;
				inputs[i].inputUnion.ki.wVk = (short)key;
				i++;

				// Simulate releasing the modifier keys.
				foreach (var modifierKey in modifierKeys) {
					inputs[i].type = NativeMethods.INPUT_KEYBOARD;
					inputs[i].inputUnion.ki.wVk = (short)modifierKey;
					inputs[i].inputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;
					i++;
				}

				UnsafeInputMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
			}
		}

		public static void SendToggledKey(int key) {
			var inputs = new NativeMethods.INPUT[2];

			// Press the key.
			inputs[0].type = NativeMethods.INPUT_KEYBOARD;
			inputs[0].inputUnion.ki.wVk = (short)key;

			// Release the key.
			inputs[1].type = NativeMethods.INPUT_KEYBOARD;
			inputs[1].inputUnion.ki.wVk = (short)key;
			inputs[1].inputUnion.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

			UnsafeInputMethods.SendInput(2, inputs, Marshal.SizeOf(inputs[0]));
		}
	}
}
