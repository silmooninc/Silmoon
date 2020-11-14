using System;
using System.Threading;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Silmoon.Net;
using Microsoft.Win32;

namespace Silmoon.Service
{
    /// <summary>
    /// ��ϵͳ������̲���
    /// </summary>
    public sealed class ServiceControl : MarshalByRefObject , IDisposable
    {
        public ServiceControl()
        {
            InitClass();
        }
        private void InitClass()
        {

        }

        public event SmServiceEventHandler OnServiceStateChange;

        void onServiceStateChange(SmServiceEventArgs e)
        {
            if (this.OnServiceStateChange != null)
            {
                this.OnServiceStateChange(this, e);
            }
        }

        /// <summary>
        /// ֹͣ����
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public bool StopService(string serviceName)
        {
            bool isOK = false;
            SmServiceEventArgs es = new SmServiceEventArgs();
            es.ServiceName = serviceName;
            es.ServiceOption = ServiceOptions.Stop;

            if (IsExisted(serviceName))
            {
                es.CompleteState = ServiceCompleteStateType.Trying;
                onServiceStateChange(es);

                if (CanStop(serviceName))
                {
                    System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                    if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        try
                        {
                            service.Stop();

                            for (int i = 0; i < 30; i++)
                            {
                                service.Refresh();
                                System.Threading.Thread.Sleep(1000);
                                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                                {
                                    es.CompleteState = ServiceCompleteStateType.Successfully;
                                    es.Error = null;
                                    isOK = true;
                                    break;
                                }
                                if (i == 30)
                                {
                                    es.Error = new Exception("�����ڿ���ʱ���ڲ�����ʱ");
                                    es.CompleteState = ServiceCompleteStateType.Timeout;
                                }
                            }
                        }
                        catch
                        {
                            es.Error = new Exception("����ֹͣʧ��");
                            es.CompleteState = ServiceCompleteStateType.UncanStop;
                        }
                    }
                    service.Close();
                }
                else
                {
                    es.Error = new Exception("�����ܿ���Ϊֹͣ");
                    es.CompleteState = ServiceCompleteStateType.UncanStop;
                }
            }
            else
            {
                es.Error = new Exception("ָ���ķ��񲻴���");
                es.CompleteState = ServiceCompleteStateType.NoExist;
            }
            onServiceStateChange(es);
            return isOK;
        }
        /// <summary>
        /// ��ʼ����
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public bool StartService(string serviceName)
        {
            bool isOK = false;
            SmServiceEventArgs es = new SmServiceEventArgs();

            es.ServiceName = serviceName;
            es.ServiceOption = ServiceOptions.Start;
            if (IsExisted(serviceName))
            {
                //�������
                es.CompleteState = ServiceCompleteStateType.Trying;
                onServiceStateChange(es);

                if (GetRunType(serviceName) != ServiceStartType.Disabled)
                {

                    //�����ǽ��õ�
                    System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                    if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                    {
                        //�����������е�
                        try { service.Start(); }
                        catch (Exception ex)
                        {
                            es.Error = ex;
                            es.CompleteState = ServiceCompleteStateType.Error;
                            onServiceStateChange(es);
                            return false;
                        }

                        for (int i = 0; i < 30; i++)
                        {
                            service.Refresh();
                            System.Threading.Thread.Sleep(1000);
                            if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                            {
                                es.CompleteState = ServiceCompleteStateType.Successfully;
                                es.Error = null;
                                onServiceStateChange(es);
                                isOK = true;
                                break;
                            }
                            if (i == 30)
                            {
                                es.Error = new Exception("�����ڿ���ʱ���ڲ�����ʱ");
                                es.CompleteState = ServiceCompleteStateType.Timeout;
                                isOK = true;
                            }
                        }
                    }
                    else
                    {
                        es.CompleteState = ServiceCompleteStateType.Successfully;
                        es.Error = new Exception("�����Ѿ�����");
                        onServiceStateChange(es);
                    }
                    service.Close();
                }
                else
                {
                    //���ǽ��õ�
                    es.Error = new Exception("�����ڽ���״̬���ܲ���");
                    es.CompleteState = ServiceCompleteStateType.Disabled;
                    onServiceStateChange(es);
                }
            }
            else
            {
                //���񲻴���
                es.Error = new Exception("ָ���ķ��񲻴���");
                es.CompleteState = ServiceCompleteStateType.NoExist;
                onServiceStateChange(es);
            }
            return isOK;
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public bool ResetService(string serviceName)
        {
            bool isOK = false;
            SmServiceEventArgs es = new SmServiceEventArgs();

            if (IsExisted(serviceName))
            {
                es.ServiceName = serviceName;
                es.ServiceOption = ServiceOptions.Reset;
                es.CompleteState = ServiceCompleteStateType.Trying;
                onServiceStateChange(es);

                if (StopService(serviceName))
                    if (StartService(serviceName)) { isOK = true; }

                //es.ServiceOption = ServiceOptions.Reset;
                //es.Error = !isOK;
                //if (!isOK) { es.CompleteState = ServiceCompleteStateType.Successfully; }
                //else { es.CompleteState = ServiceCompleteStateType.Error; }
                //es.Error = !isOK;
                //onServiceStateChange(es);
            }
            else
            {
                es.ServiceName = serviceName;
                es.ServiceOption = ServiceOptions.Reset;
                es.CompleteState = ServiceCompleteStateType.NoExist;
                es.Error = new Exception("ָ���ķ��񲻴���");
                onServiceStateChange(es);
            }

            return isOK;
        }

        /// <summary>
        /// �첽���Ʒ���
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <param name="so">����ѡ��</param>
        public void AsyncService(string serviceName, ServiceOptions so)
        {
            ServiceThreadMethod stm = new ServiceThreadMethod(this, serviceName, so);
            Thread _th = new Thread(new ThreadStart(stm.RunThread));
            _th.Start();
        }
        private class ServiceThreadMethod
        {
            public ServiceControl _ss;
            public string _serviceName;
            public ServiceOptions _so;

            public void RunThread()
            {
                if (_ss != null)
                {
                    if (_serviceName != null)
                    {
                        switch (_so)
                        {
                            case ServiceOptions.Start:
                                _ss.StartService(_serviceName);
                                break;
                            case ServiceOptions.Stop:
                                _ss.StopService(_serviceName);
                                break;
                            case ServiceOptions.Reset:
                                _ss.ResetService(_serviceName);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            public ServiceThreadMethod(ServiceControl ss, string serviceName, ServiceOptions so)
            {
                _ss = ss;
                _serviceName = serviceName;
                _so = so;
            }
        }

        /// <summary>
        /// ���ط����Ƿ��ܹ�ֹͣ
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public bool CanStop(string serviceName)
        {
            if (IsExisted(serviceName))
            {
                ServiceController cs = new ServiceController();
                cs.ServiceName = serviceName;
                if (cs.CanStop)
                {
                    cs.Close();
                    return true;
                }
                else
                {
                    cs.Close();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// �����Ƿ����
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public static bool IsExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == serviceName.ToLower())
                { return true; }
            }
            return false;
        }
        /// <summary>
        /// ���÷���������ʽ
        /// </summary>
        /// <param name="_type">����</param>
        /// <param name="serviceName">������</param>
        public static void SetRunType(ServiceStartType _type, string serviceName)
        {
            RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName + "\\", true);
            k.SetValue("Start", Convert.ToInt32(_type), RegistryValueKind.DWord);
            k.Close();
        }
        /// <summary>
        /// ��ȡ����������ʽ
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <returns></returns>
        public static ServiceStartType GetRunType(string serviceName)
        {
            if (IsExisted(serviceName))
            {
                RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName + "\\", true);
                ServiceStartType sstype = ((ServiceStartType)Convert.ToInt32(k.GetValue("Start")));
                k.Close();
                return sstype;
            }
            else
                throw new Exception("���񲻴���");
        }
        /// <summary>
        /// ���÷�������
        /// </summary>
        /// <param name="serviceName">������</param>
        /// <param name="_type">����</param>
        public static void SetServiceType(string serviceName, ServiceType _type)
        {
            RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName + "\\", true);
            k.SetValue("Type", Convert.ToInt32(_type), RegistryValueKind.DWord);
            k.Close();
        }
        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static ServiceType GetServiceType(string serviceName)
        {
            if (IsExisted(serviceName))
            {
                RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName + "\\", true);
                ServiceType sstype = ((ServiceType)Convert.ToInt32(k.GetValue("Type")));
                k.Close();
                return sstype;
            }
            else
                throw new Exception("���񲻴���");
        }

        #region IDisposable ��Ա

        public void Dispose()
        {

        }

        #endregion
    }
    /// <summary>
    /// ΪSmServiceEventHandler�ṩ�¼�����
    /// </summary>
    [Serializable]
    public class SmServiceEventArgs : System.EventArgs
    {
        private string _serviceName;
        private Exception _error = null;
        private ServiceCompleteStateType _completeState = ServiceCompleteStateType.None;
        private ServiceOptions _serviceOption = ServiceOptions.None;

        public ServiceOptions ServiceOption
        {
            get { return _serviceOption; }
            set { _serviceOption = value; }
        }
        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        public Exception Error
        {
            get { return _error; }
            set { _error = value; }
        }
        public ServiceCompleteStateType CompleteState
        {
            get { return _completeState; }
            set { _completeState = value; }
        }
    }
    /// <summary>
    /// Service�¼��й�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SmServiceEventHandler(object sender, SmServiceEventArgs e);
    /// <summary>
    /// Service ״̬
    /// </summary>
    public enum ServiceCompleteStateType
    {
        Successfully = 1,
        Unsucceed = 2,
        NoExist = 3,
        Timeout = 4,
        UncanStop = 5,
        Error = 6,
        Trying = 7,
        Disabled = 8,
        None = 0,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ServiceOptions
    {
        None = 0,
        Stop = 1,
        Start = 2,
        Reset = 3
    }
    /// <summary>
    /// ������������ʽ
    /// </summary>
    public enum ServiceStartType
    {
        Starting = 0,
        System = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4,
        NoExist = 5,
    }
    /// <summary>
    /// ��������ö��
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// һ�������
        /// </summary>
        Normal = 16,
        /// <summary>
        /// ������������滥��
        /// </summary>
        ShowOnDesktop = 272,
    }
}