using System;
using System.Collections.Generic;
using System.Text;

namespace Silmoon.Security
{
    public class SmHash
    {
        public SmHash()
        {

        }

        public static string Get16MD5(string strSource)
        {
            //new 
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            //��ȡ�����ֽ����� 
            byte[] bytResult = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strSource));

            //ת�����ַ�������ȡ9��25λ 
            string strResult = BitConverter.ToString(bytResult, 4, 8);
            //ת�����ַ�����32λ 
            //string strResult = BitConverter.ToString(bytResult); 

            //BitConverterת���������ַ�������ÿ���ַ��м����һ���ָ�������Ҫȥ���� 
            strResult = strResult.Replace("-", "");
            return strResult;
        }
        /// <summary> 
        /// ����MD5��32λ����
        /// </summary> 
        /// <param name="strSource">��Ҫ���ܵ�����</param> 
        /// <returns>����32λ���ܽ��</returns> 
        public static string Get32MD5(string strSource)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strSource, "MD5");
        }

        public static string GenerateCheckCodeNum(int codeCount)
        {
            return GenerateCheckCodeNum(codeCount, 1);
        }
        /// <summary>
        /// ������������ַ���
        /// </summary>
        /// <param name="codeCount">�����ɵ�λ��</param>
        /// <param name="seed">�������</param>
        /// <returns>���ɵ������ַ���</returns>
        public static string GenerateCheckCodeNum(int codeCount, int seed)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + seed;
            seed++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> seed)));
            for (int i = 0; i < codeCount; i++)
            {
                int num = random.Next();
                str = str + ((char)(0x30 + ((ushort)(num % 10)))).ToString();
            }
            return str;
        }
        public static string GenerateCheckCode(int codeCount)
        {
            return GenerateCheckCode(codeCount, 1);
        }
        /// <summary>
        /// ���������ĸ�ַ���(������ĸ���)
        /// </summary>
        /// <param name="codeCount">�����ɵ�λ��</param>
        /// <param name="seed">�������</param>
        /// <returns>���ɵ���ĸ�ַ���</returns>
        public static string GenerateCheckCode(int codeCount, int seed)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + seed;
            seed++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> seed)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }
    }
}
