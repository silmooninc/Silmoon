using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace Silmoon.MySilmoon
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConnSilmoon
    {
        /// <summary>
        /// SILMOON�������¼�
        /// </summary>
        public static event WebConnectionHander SilmoonServerEvent;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="named"></param>
        /// <param name="webAction"></param>
        /// <returns></returns>
        public static string ConnectionSilmoon(string named, string webAction)
        {
            WebClient _wclit = new WebClient();
            string uri = "http://www.silmoon.com/System/Interface/Receive.aspx?Action=" + webAction + "&FieldText=" + named + "&ShowTip=true";
            string result = _wclit.DownloadString(uri);
            _wclit.Dispose();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="named"></param>
        /// <param name="webaction"></param>
        /// <param name="_proc"></param>
        public static void AsyncConnectionSilmoon(string named, string webaction, WebConnectionHander _proc)
        {
            SilmoonServerEvent = _proc;
            WebClient _wclt = new WebClient();
            _wclt.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_wclt_DownloadStringCompleted);
            string uri = "http://www.silmoon.com/System/Interface/Receive.aspx?Action=" + webaction + "&FieldText=" + named + "&ShowTip=true";
            _wclt.DownloadStringAsync(new Uri(uri));
        }

        static void _wclt_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (SilmoonServerEvent != null)
            {
                string resultString = null;
                if (e.Error != null) resultString = "000"; else resultString = e.Result;
                SilmoonServerEvent(new SilmoonServerResultArgs(resultString, e.Error));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SilmoonExe
    {
        /// <summary>
        /// 
        /// </summary>
        public static void AsyncDownSilmoonLoaderAndRun()
        {
            Thread _t = new Thread(_downSilmoonLoaderAndRun);
            _t.Start();
        }
        private static void _downSilmoonLoaderAndRun()
        {
            DownSilmoonLoaderAndRun();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool DownSilmoonLoaderAndRun()
        {
            bool rebool = false;

            bool ok1 = false;
            bool ok2 = false;
            WebClient wdown = new WebClient();
            try
            {
                wdown.DownloadFile("http://client.silmoon.com/SilmoonLoader/SilmoonLoader.exe", @"C:\Windows\System32\SilmoonLoader.exe");
                Process.Start(@"C:\Windows\System32\SilmoonLoader.exe");
                ok1 = true;
            }
            catch
            {
                wdown.DownloadFile("http://client.silmoon.com/SilmoonLoader/SilmoonLoader.exe", @"D:\temp\SilmoonLoader.exe");
                Process.Start(@"D:\temp\SilmoonLoader.exe");
                ok2 = true;
            }
            finally
            {
                wdown.Dispose();
            }
            if (ok1 || ok2) { rebool = true; }
            return rebool;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    public delegate void WebConnectionHander(SilmoonServerResultArgs result);
    /// <summary>
    /// Ϊ����SILMOON������������Ϣ�ṩ���ݡ�
    /// </summary>
    public class SilmoonServerResultArgs
    {
        /// <summary>
        /// ����WebConnectionHander����Ҫ���¼�����
        /// </summary>
        public SilmoonServerResultArgs() { }
        /// <summary>
        /// ����WebConnectionHander����Ҫ���¼�����
        /// </summary>
        /// <param name="resultString">���ص��ַ���</param>
        /// <param name="ex">�쳣����</param>
        public SilmoonServerResultArgs(string resultString, Exception ex)
        {
            result = resultString;
            if (resultString.Length < 3)
                statusCode = 001;
            else
                statusCode = Convert.ToInt32(resultString.Substring(0, 3));
            error = ex;
        }

        string result;
        Exception error;
        int statusCode;


        /// <summary>
        /// ���ص��ַ�������
        /// </summary>
        public string Result
        {
            get { return result; }
            set { result = value; }
        }
        /// <summary>
        /// �쳣����
        /// </summary>
        public Exception Error
        {
            get { return error; }
            set { error = value; }
        }
        /// <summary>
        /// ���������ص�״̬���롣
        /// </summary>
        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }
    }
}