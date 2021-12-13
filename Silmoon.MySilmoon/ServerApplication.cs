using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Silmoon.Threading;

namespace Silmoon.MySilmoon
{
    /// <summary>
    /// �����²�Ʒ�����⹫�����Խ�������
    /// </summary>
    public class ServerApplication : RunningAble, IServerApplication
    {
        private string _productString = "NULL";
        private int _revision = 0;
        private string _releaseVersion = "0";
        private bool _initProduceInfo = false;
        private string _userIdentity = "#undefined";

        public event OutputTextMessageHandler OnOutputLine;
        public event OutputTextMessageHandler OnInputLine;
        public event ThreadExceptionEventHandler OnException;

        /// <summary>
        /// ��ʶ��Ʒ�����ַ���
        /// </summary>
        public string ProductString
        {
            get { return _productString; }
            set { _productString = value; }
        }
        /// <summary>
        /// ��Ʒ�������
        /// </summary>
        public int Revision
        {
            get { return _revision; }
            set { _revision = value; }
        }
        /// <summary>
        /// �汾��
        /// </summary>
        public string ReleaseVersion
        {
            get { return _releaseVersion; }
            set { _releaseVersion = value; }
        }
        public string UserIdentity
        {
            get { return _userIdentity; }
            set { _userIdentity = value; }
        }

        public ServerApplication()
        {

        }


        public void OutputLine()
        {
            OutputLine(null, 0);
        }
        public void OutputLine(object message, int flag = 0)
        {
            if (message is null)
                OutputLine(null, flag);
            else
                OutputLine(message.ToString(), flag);
        }
        public void OutputLine(string message, int flag = 0)
        {
            OnOutputLine?.Invoke(message, flag);
        }
        public void InputLine(string message, int flag = 0)
        {
            OnInputLine?.Invoke(message, flag);
        }

        /// <summary>
        /// ��GBC�����̴߳����¼�����GBC��OnThreadException�¼����񲢴���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void onThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (OnException != null) OnException(sender, e);
        }

        /// <summary>
        /// ʹ��һ���첽����ʹ�ò�Ʒ�ַ���ʶ�Ͱ汾Revisionʹ��MyConfigure.GetRemoteVersion��֤������OnValidateVersion������֤�����
        /// </summary>
        public void AsyncValidateVersion()
        {
            ThreadHelper.ExecAsync(delegate ()
            {
                if (OnValidateVersion != null)
                {
                    var result = MyConfigure.GetRemoteVersion(_productString, _userIdentity);
                    OnValidateVersion(result);
                }
                else
                {
                    ///��û�жԻص���֤�¼����̵��õ�ʱ�򣬶�ȥ��������֤���̵Ĵ���������Ӧ���׳�һ���쳣��
                }
            });
        }
        /// <summary>
        /// ʹ�ò�Ʒ�ַ���ʶ�Ͱ汾Revisionʹ��MyConfigure.GetRemoteVersion��֤������OnValidateVersion������֤����������̡߳�
        /// </summary>

        /// <summary>
        /// ֹͣĿǰ����ǰ̨������Ӧ�ĺ�̨����
        /// </summary>
        /// <param name="serviceName">��̨��������</param>
        //public void StopAppService(string serviceName)
        //{
        //    if (Environment.UserInteractive)
        //    {
        //        OutputLine("GBC : Application runing at interactive mode...", -999);
        //        if (ServiceControl.IsExisted(serviceName))
        //        {
        //            OutputLine("GBC : Application associate service(" + serviceName + ") is exist...", -999);
        //            using (ServiceController sc = new ServiceController(serviceName))
        //            {
        //                sc.Refresh();
        //                if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
        //                {
        //                    OutputLine("GBC : Application associate service(" + serviceName + ") is running, shutdown it...", -999);
        //                    sc.Stop();
        //                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
        //                    OutputLine("GBC : Application associate service(" + serviceName + ") has been shutdown...", -999);
        //                }
        //                else
        //                {
        //                    OutputLine("GBC : Application associate service(" + serviceName + ") not running or can't stop it...", -999);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        /// <param name="productString">ָ����Ʒ�����ַ���</param>
        /// <param name="revision">ָ��������Ʒ�����</param>
        public bool InitProductInfo(string productString, int revision, string releaseVersion = "0")
        {
            if (!_initProduceInfo)
            {
                _productString = productString;
                _revision = revision;
                _releaseVersion = releaseVersion;
                _initProduceInfo = true;
                return true;
            }
            else
                return false;
        }
    }
    public delegate void OutputTextMessageHandler(string message, int flag);
}