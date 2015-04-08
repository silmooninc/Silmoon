using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Silmoon.Net.SmProtocol
{
    /// <summary>
    /// SMЭ�鴦�����
    /// </summary>
    public class SmPackectProtocol
    {
        /// <summary>
        /// ��ʵ��Э�鴦�����
        /// </summary>
        public SmPackectProtocol()
        {

        }
        /// <summary>
        /// ��ȡSMЭ��ͷ�ṹ����
        /// </summary>
        /// <param name="packect">����</param>
        /// <returns></returns>
        public ProtocalHeader IsProtocolHeader(byte[] packect)
        {
            ProtocalHeader result;
            result.PackectLength = 0;
            result.IsSmProtocol = false;


            string stringData = Encoding.Default.GetString(packect);
            if (stringData.Length < 5) return result;

            int startC = stringData.IndexOf("_sm_");
            if (startC == -1) return result;

            stringData = stringData.Substring(startC, stringData.Length - startC);

            if (stringData.Substring(0, 4) != "_sm_") return result;
            if (stringData.Substring(stringData.Length - 4, 4) != "_end") return result;
            string[] packInfoArr = stringData.Split(new string[] { "_" }, StringSplitOptions.None);
            if (packInfoArr.Length < 4) return result;
            if (Convert.ToInt64(packInfoArr[2]) == 0) return result;
            try
            {
                result.IsSmProtocol = true;
                result.PackectLength = Convert.ToInt64(packInfoArr[2]);
            }
            catch
            {
                result.IsSmProtocol = false;
                result.PackectLength = 0;
            }

            return result;
        }
        /// <summary>
        /// ����SMЭ��ͷ�����״̬��ȡ����
        /// </summary>
        /// <param name="status">SMЭ��װ״̬����</param>
        /// <param name="byteData">����</param>
        /// <returns></returns>
        public byte[] ReadFormSmProtocol(ref ProtocalStatusInfo status, byte[] byteData)
        {
            if (byteData.Length == status.PackectLength)
            {
                status.Received = false;
                return byteData;
            }
            return null;
        }
        /// <summary>
        /// ����һ������SMЭ��ͷ�����ݰ�
        /// </summary>
        /// <param name="byteData">���ݰ�����������</param>
        /// <returns></returns>
        public byte[] MakeByteData(byte[] byteData)
        {
            if (byteData.Length == 0) { return null; }

            byte[] headerData = Encoding.Default.GetBytes("_sm_" + byteData.Length + "_end");
            ArrayList resultArr = new ArrayList();
            foreach (byte b in headerData) resultArr.Add(b);
            foreach (byte b in byteData) resultArr.Add(b);
            byte[] resultBytes = (byte[])resultArr.ToArray(typeof(byte));
            resultArr.Clear();
            return resultBytes;
        }
    }
    /// <summary>
    /// ���ڱ���SMЭ���״̬
    /// </summary>
    public struct ProtocalStatusInfo
    {
        /// <summary>
        /// �Ƿ��Ѿ����ܵ���SMЭ��ͷ
        /// </summary>
        public bool Received;
        /// <summary>
        /// SMЭ��ͷ�б�ʾ�����ݳ���
        /// </summary>
        public long PackectLength;
    }
    /// <summary>
    /// SMЭ��ͷ�ṹ
    /// </summary>
    public struct ProtocalHeader
    {
        public long PackectLength;
        public bool IsSmProtocol;
    }
}
