using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Silmoon.Windows.Win32.Hooking
{
    /// <summary> 
    /// ������������õ�һ���������г�������м����¼� 
    /// ��������һ����KeyEventArgs��MouseEventArgs������.NET�¼��Ա��������ʹ����Щ��Ϣ 
    /// </summary> 
    public class GlobalKeyBoardHook
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        //ȫ�ֵ��¼� 
        public event KeyEventHandler OnKeyDownEvent;
        public event KeyEventHandler OnKeyUpEvent;
        public event KeyPressEventHandler OnKeyPressEvent;

        static int hKeyboardHook = 0; //���̹��Ӿ��

        //��곣�� 
        public const int WH_KEYBOARD_LL = 13; //keyboard hook constant

        HookProc KeyboardHookProcedure; //�������̹����¼�����.

        //�������̹��ӵķ��ͽṹ���� 
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode; //��ʾһ����1��254������Ƽ����� 
            public int scanCode; //��ʾӲ��ɨ���� 
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        //װ�ù��ӵĺ��� 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //ж�¹��ӵĺ��� 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //��һ�����ҵĺ��� 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        //��ǰ���µļ�
        public List<Keys> preKeys = new List<Keys>();

        /// <summary> 
        /// ī�ϵĹ��캯�����쵱ǰ���ʵ�����Զ�����������. 
        /// </summary> 
        public GlobalKeyBoardHook()
        {

        }

        //��������. 
        ~GlobalKeyBoardHook()
        {
            try { Stop(); }
            catch { }
        }

        public void Start()
        {
            //��װ���̹��� 
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                //hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                Process curProcess = Process.GetCurrentProcess();
                ProcessModule curModule = curProcess.MainModule;

                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(curModule.ModuleName), 0);

                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx ist failed.");
                }
            }
        }

        public void Stop()
        {
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            //���ж�¹���ʧ�� 
            if (!(retKeyboard)) throw new Exception("UnhookWindowsHookEx failed.");
        }

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {

            if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null || OnKeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //����OnKeyDownEvent �� OnKeyPressEvent��Ϊnullʱ,ctrl alt shift keyupʱ preKeys
                //�еĶ�Ӧ�ļ�����                   
                if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    if (IsCtrlAltShiftKeys(keyData) && preKeys.IndexOf(keyData) == -1)
                    {
                        preKeys.Add(keyData);
                    }
                }
                //����OnKeyDownEvent 
                if (OnKeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));

                    OnKeyDownEvent(this, e);
                }

                //����OnKeyPressEvent 
                if (OnKeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        OnKeyPressEvent(this, e);
                    }
                }

                //����OnKeyDownEvent �� OnKeyPressEvent��Ϊnullʱ,ctrl alt shift keyupʱ preKeys
                //�еĶ�Ӧ�ļ�ɾ��
                if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    if (IsCtrlAltShiftKeys(keyData))
                    {

                        for (int i = preKeys.Count - 1; i >= 0; i--)
                        {
                            if (preKeys[i] == keyData)
                            {
                                preKeys.RemoveAt(i);
                            }
                        }

                    }
                }
                //����OnKeyUpEvent 
                if (OnKeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));
                    OnKeyUpEvent(this, e);
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }



        private Keys GetDownKeys(Keys key)
        {
            Keys rtnKey = Keys.None;
            foreach (Keys keyTemp in preKeys)
            {
                switch (keyTemp)
                {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        rtnKey = rtnKey | Keys.Control;
                        break;
                    case Keys.LMenu:
                    case Keys.RMenu:
                        rtnKey = rtnKey | Keys.Alt;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        rtnKey = rtnKey | Keys.Shift;
                        break;
                    default:
                        break;
                }
            }
            rtnKey = rtnKey | key;

            return rtnKey;
        }

        private Boolean IsCtrlAltShiftKeys(Keys key)
        {

            switch (key)
            {
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return true;
                default:
                    return false;
            }
        }
    }
}
