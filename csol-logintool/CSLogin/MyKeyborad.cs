using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CommonQ
{
    class MyKeyborad
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 MapVirtualKey(UInt32 virtualKeyCode, UInt32 uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte vVK,byte bScan, UInt32 dwFlags, IntPtr dwExtraInfo);

        const int KEYEVENTF_KEYUP = 0x0002;

        const int delaytime = 40;

        public static bool keyPress(VirtualKeyCode keycode)
        {
            keybd_event((byte)keycode, (byte)MapVirtualKey((UInt32)keycode, 0), 0, IntPtr.Zero);
            Thread.Sleep(delaytime);
            keybd_event((byte)keycode, (byte)MapVirtualKey((UInt32)keycode, 0), KEYEVENTF_KEYUP, IntPtr.Zero);
            return true;
        }

        public static bool keyDown(VirtualKeyCode keycode)
        {
            keybd_event((byte)keycode, (byte)MapVirtualKey((UInt32)keycode, 0), 0, IntPtr.Zero);
            return true;
        }

        public static bool keyUp(VirtualKeyCode keycode)
        {
            keybd_event((byte)keycode, (byte)MapVirtualKey((UInt32)keycode, 0), KEYEVENTF_KEYUP, IntPtr.Zero);
            return true;
        }
    }
}
