using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Silmoon.Secure
{
    public class EncryptString
    {
        [Obsolete]
        public static string EncryptSilmoonBinary(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            string restring = string.Empty;
            foreach (byte b in bytes)
            {
                restring += b.ToString() + ",";
            }
            return restring;
        }
        [Obsolete]
        public static string DiscryptSilmoonBinary(string s)
        {
            string[] str = s.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            byte[] rebytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                rebytes[i] = (byte)int.Parse(str[i]);
            }
            return Encoding.UTF8.GetString(rebytes);
        }
    }
}
