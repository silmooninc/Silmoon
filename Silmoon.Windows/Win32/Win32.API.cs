using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using Silmoon.Windows.Win32.API;


namespace Silmoon.Windows.Win32.API
{
    public struct WindowInfo
    {
        public IntPtr hWnd;
        public string szWindowName;
        public string szClassName;
    }

    // ����ṹ�彫�ᴫ�ݸ�API��ʹ��StructLayout(...���ԣ�ȷ�����еĳ�Ա�ǰ�˳�����еģ�C#���������������е����� 
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TokPriv1Luid
    {
        public int Count;
        public long Luid;
        public int Attr;
    }

}