using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using Silmoon.MySilmoon.Instance;
using Silmoon.Service;
using Silmoon.Threading;

namespace Silmoon.MySilmoon
{
    /// <summary>
    /// �����²�Ʒ�����⹫�����Խ�������
    /// </summary>
    public class SilmoonProductGBCInternat : RunningAble, ISilmoonProductGBCInternat
    {
        private string _productString = "NULL";
        private int _revision = 0;
        private string _releaseVersion = "0";
        private bool _initProduceInfo = false;
        private string _userIdentity = "#undefined";

        public event OutputTextMessageHandler OnOutput;
        public event OutputTextMessageHandler OnInput;
        public event ThreadExceptionEventHandler OnException;
        /// <summary>
        /// AsyncValidateVersion invoked.
        /// </summary>
        public event Action<VersionResult> OnValidateVersion;

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

        public SilmoonProductGBCInternat()
        {

        }


        public void Output(string message)
        {
            Output(message, 0);
        }
        public void Output(string message, int flag)
        {
            OnOutput?.Invoke(message, flag);
        }
        public void Input(string message)
        {
            Input(message, 0);
        }
        public void Input(string message, int flag)
        {
            OnInput?.Invoke(message, flag);
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
        public VersionResult ValidateVersion()
        {
            return MyConfigure.GetRemoteVersion(_productString, _userIdentity);
        }

        /// <summary>
        /// ֹͣĿǰ����ǰ̨������Ӧ�ĺ�̨����
        /// </summary>
        /// <param name="serviceName">��̨��������</param>
        public void StopAppService(string serviceName)
        {
            if (Environment.UserInteractive)
            {
                Output("GBC : Application runing at interactive mode...", -999);
                if (ServiceControl.IsExisted(serviceName))
                {
                    Output("GBC : Application associate service(" + serviceName + ") is exist...", -999);
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        sc.Refresh();
                        if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
                        {
                            Output("GBC : Application associate service(" + serviceName + ") is running, shutdown it...", -999);
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                            Output("GBC : Application associate service(" + serviceName + ") has been shutdown...", -999);
                        }
                        else
                        {
                            Output("GBC : Application associate service(" + serviceName + ") not running or can't stop it...", -999);
                        }
                    }
                }
            }
        }

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