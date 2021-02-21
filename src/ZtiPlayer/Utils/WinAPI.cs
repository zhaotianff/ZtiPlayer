﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Utils
{
    struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpVerb;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpParameters;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    struct KBDLLHOOKSTRUCT
    {
        int vkCode;
        int scanCode;
        int flags;
        int time;
        IntPtr dwExtraInfo;
    }

    class WinAPI
    {      
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        [DllImport("shell32.dll")]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        public static void ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }

        [DllImport("User32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("User32.dll")]
        public static extern int GetKeyNameText(int lParam,StringBuilder lpString,int cchSize);

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
