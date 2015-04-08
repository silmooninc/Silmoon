using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Silmoon;
using Silmoon.Net;
using System.Collections;
using Silmoon.Net.SmProtocol;
using Silmoon.Threading;

namespace Silmoon.Net
{
    /// <summary>
    /// ����TCP�����ṩSMЭ�����͵�ͨѶ���ܣ�
    /// </summary>
    public class SmTcp : IDisposable
    {
        internal TcpStruct _localTcpStruct;
        private TcpStruct _remoteTcpStruct;

        private bool _listenning = false;
        private Encoding _dataEncoding = Encoding.Default;
        private TcpMode _tcpMode = TcpMode.Unknown;

        bool _useBlockRead = false;

        /// <summary>
        /// �Ƿ�ʹ����ϵķ����ӷ�������ȡ���ݡ�
        /// </summary>
        public bool UseBlockRead
        {
            get { return _useBlockRead; }
            set { _useBlockRead = value; }
        }


        /// <summary>
        /// ������TCP�¼�
        /// </summary>
        public event TcpOptionEventHander OnTcpEvents;
        /// <summary>
        /// �����յ�����
        /// </summary>
        public event TcpReceiveDataEventHander OnReceivedData;
        /// <summary>
        /// �����������ʱ�򣬶��������첽�쳣����
        /// </summary>
        public event TcpOnErrorEventHander OnError;
        /// <summary>
        /// �����������¼�ʱ��ָ������
        /// </summary>
        public event TcpOnConnectionEventHander OnConnectionEvent;

        TcpClient _tc;
        TcpListener _tl;
        NetworkStream _ns;

        ArrayList _byteCache = new ArrayList();
        ArrayList _tcp_Reader_Array = new ArrayList();

        SmPackectProtocol _protocol = new SmPackectProtocol();
        Thread _Async_thread;
        ProtocalStatusInfo _netStatusInfo;

        bool _listenWork = false;

        /// <summary>
        /// ��ȡ�����õ�ǰ����ʹ�õı��뷽ʽ
        /// </summary>
        public Encoding DataEncoding
        {
            get { return _dataEncoding; }
            set { _dataEncoding = value; }
        }
        /// <summary>
        /// ��ȡ����Tcp��Ϣ
        /// </summary>
        public TcpStruct LocalTcpStruct
        {
            get { return _localTcpStruct; }
            set { _localTcpStruct = value; }
        }
        /// <summary>
        /// ��ȡԶ��Tcp��Ϣ
        /// </summary>
        public TcpStruct RemoteTcpStruct
        {
            get { return _remoteTcpStruct; }
            set { _remoteTcpStruct = value; }
        }
        /// <summary>
        /// ��ȡ��ǰ�Ƿ�Ϊ�����˿�״̬
        /// </summary>
        public bool Listenning
        {
            get { return _listenning; }
        }
        /// <summary>
        /// ��ȡ��ǰ�Ƿ��Ѿ����ӵ�Զ�̼����
        /// </summary>
        public bool Connected
        {
            get
            {
                if (_tc == null) return false;
                else return _tc.Connected;
            }
        }
        /// <summary>
        /// ��ǰ������TCPģʽ
        /// </summary>
        public TcpMode TcpMode
        {
            get { return _tcpMode; }
        }
        /// <summary>
        /// ��ȡ���������û�
        /// </summary>
        public __listen__readSmtcp[] Connections
        {
            get { return (__listen__readSmtcp[])_tcp_Reader_Array.ToArray(typeof(__listen__readSmtcp)); }
        }

        /// <summary>
        /// ����SMTCP��ʵ��
        /// </summary>
        public SmTcp()
        {

        }
        /// <summary>
        /// ��ָ���Ķ˿ڿ�ʼ��������
        /// </summary>
        /// <param name="ip">ָ��Զ�̵ļ����IP</param>
        /// <param name="port">ָ��Զ�̼�����˿�</param>
        public void StartListen(IPAddress ip, int port)
        {
            TcpStruct tstr;
            tstr.IP = ip;
            tstr.Port = port;
            StartListen(tstr);
        }
        /// <summary>
        /// ��ָ���Ķ˿ڿ�ʼ��������
        /// </summary>
        /// <param name="tstr">ָ��Զ�̵ļ����Tcp�ṹ</param>
        public void StartListen(TcpStruct tstr)
        {
            try
            {
                if (_tl == null) _tl = new TcpListener(tstr.IP, tstr.Port);
                _tl.Start();
            }
            catch (Exception ex) { onError(tstr, _remoteTcpStruct, TcpError.UncreateListen, ex, TcpOptionType.CreateListen, null); return; }

            _localTcpStruct.IP = tstr.IP;
            _localTcpStruct.Port = tstr.Port;
            onTcpEvents(_localTcpStruct, _remoteTcpStruct, TcpOptionType.StartListen, null);
            _tcpMode = TcpMode.Server;
            ReadDataFromListen();
        }
        /// <summary>
        /// �첽��ָ���Ķ˿ڿ�ʼ��������
        /// </summary>
        /// <param name="ip">ָ��Զ�̵ļ����IP</param>
        /// <param name="port">ָ��Զ�̼�����˿�</param>
        public void AsyncStartListen(IPAddress ip, int port)
        {
            _localTcpStruct.IP = ip;
            _localTcpStruct.Port = port;

            Threads.ExecAsync(async_th_listen);
        }
        /// <summary>
        /// �첽���ӵ�Զ�̷�����
        /// </summary>
        /// <param name="endPoint">Զ���ս��</param>
        public void AsyncConnectTo(IPEndPoint endPoint)
        {
            _remoteTcpStruct.IP = endPoint.Address;
            _remoteTcpStruct.Port = endPoint.Port;

            Threads.ExecAsync(async_th_connect);
        }
        /// <summary>
        /// �첽���ӵ�һ��֧��SMЭ��ļ�����˿�
        /// </summary>
        /// <param name="ip">Ŀ��IP</param>
        /// <param name="port">Ŀ��˿�</param>
        public void AsyncConnectTo(IPAddress ip, int port)
        {
            _remoteTcpStruct.IP = ip;
            _remoteTcpStruct.Port = port;

            Threads.ExecAsync(async_th_connect);
        }
        /// <summary>
        /// �첽���ӵ�һ��֧��SMЭ��ļ�����˿�
        /// </summary>
        /// <param name="tstr">Զ��TcpStruct</param>
        public void AsyncConnectTo(TcpStruct tstr)
        {
            _remoteTcpStruct.IP = tstr.IP;
            _remoteTcpStruct.Port = tstr.Port;

            Threads.ExecAsync(async_th_connect);
        }
        /// <summary>
        /// ֹͣ�ڱ����ļ���
        /// </summary>
        public void StopListen(bool closeConnect)
        {
            if (closeConnect) CloseConnect();
            if (_listenning)
            {
                _tl.Stop();
                onStopListen();
            }
            _tcpMode = TcpMode.Unknown;
        }
        /// <summary>
        /// ���ӵ�һ��Զ�̷�����
        /// </summary>
        /// <param name="endPoint">Զ���ս��</param>
        /// <returns></returns>
        public TcpResult ConnectTo(IPEndPoint endPoint)
        {
            return ConnectTo(endPoint.Address, endPoint.Port);
        }
        /// <summary>
        /// ���ӵ�һ��֧��SMЭ��ļ�����˿�
        /// </summary>
        /// <param name="ip">Ŀ��IP</param>
        /// <param name="port">Ŀ��˿�</param>
        public TcpResult ConnectTo(IPAddress ip, int port)
        {
            _remoteTcpStruct.IP = ip;
            _remoteTcpStruct.Port = port;
            return ConnectTo(_remoteTcpStruct);
        }
        /// <summary>
        /// ���ӵ�һ��֧��SMЭ��ļ�����˿�
        /// </summary>
        /// <param name="tstr">ָ��Զ�̵ļ����Tcp�ṹ</param>
        public TcpResult ConnectTo(TcpStruct tstr)
        {
            TcpResult result = new TcpResult();
            result.Success = false;
            onTcpEvents(this._localTcpStruct, this._remoteTcpStruct, TcpOptionType.Connecting, null);
            if (Connected)
            {
                onError(_localTcpStruct, _remoteTcpStruct, TcpError.TcpClientIsConnected, null, TcpOptionType.Connecting, null);
                result.Success = false;
                result.Error = TcpError.TcpClientIsConnected;
                return result;
            }
            _tc = new TcpClient();
            try
            {
                _tc.Connect(tstr.IP, tstr.Port);
                _tcpMode = TcpMode.Client;
                _ns = _tc.GetStream();
                result.Success = true;
            }
            catch (Exception ex)
            {
                onError(_localTcpStruct, _remoteTcpStruct, TcpError.ServerOffline, ex, TcpOptionType.Connecting, null);
                result.Error = TcpError.ServerOffline;
                result.Success = false;
                return result;
            }
            _remoteTcpStruct.IP = tstr.IP;
            _remoteTcpStruct.Port = tstr.Port;

            FormatIPStringToTcpStruct(_tc.Client.LocalEndPoint.ToString(), ref _localTcpStruct);
            onTcpEvents(_localTcpStruct, _remoteTcpStruct, TcpOptionType.Connected, null);
            if (!UseBlockRead)
                Threads.ExecAsync(ReadDataFromConnectRemote);
            return result;
        }
        /// <summary>
        /// ��Է��������ݣ����ڿͻ�������
        /// </summary>
        /// <param name="byteData">��������</param>
        public void SendData(byte[] byteData)
        {
            SendData(byteData, -1);
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="byteData">��������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendData(byte[] byteData, int clientID)
        {
            if (byteData.Length == 0) return;

            if (_tcpMode == TcpMode.Client)
            {
                if (!Connected) { onError(_localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, null); return; }
                byte[] bytePackect = _protocol.MakeByteData(byteData);
                _ns.Write(bytePackect, 0, bytePackect.Length);
            }
            else if (_tcpMode == TcpMode.Server)
            {
                __listen__readSmtcp pclinet = GetListenClient(clientID);
                if (pclinet != null)
                    pclinet.SendData(byteData);
                else
                    onError(_localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, pclinet);
            }
            else
                onError(_localTcpStruct, _remoteTcpStruct, TcpError.UnknownModeOrNotConnected, null, TcpOptionType.SendData, null);
        }
        /// <summary>
        /// ��Է������ַ��������ڿͻ�������
        /// </summary>
        /// <param name="s">�ַ�������</param>
        public void SendString(string s)
        {
            SendString(s, -1);
        }
        /// <summary>
        /// ��Է������ַ���
        /// </summary>
        /// <param name="s">�ַ�������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendString(string s, int clientID)
        {
            SendData(DataEncoding.GetBytes(s), clientID);
        }
        /// <summary>
        /// ��ȡ�����Լ����Ĵ�������
        /// </summary>
        /// <param name="clientID">����ID</param>
        /// <returns></returns>
        public __listen__readSmtcp GetListenClient(int clientID)
        {
            foreach (object obj in _tcp_Reader_Array)
            {
                if (obj != null && ((__listen__readSmtcp)obj).ClientID == clientID)
                    return (__listen__readSmtcp)obj;
            }
            return null;
        }
        /// <summary>
        /// �رյ�ǰ��TcpClient���ӣ�����Ǽ���ģʽ����ر���������
        /// </summary>
        public void CloseConnect()
        {
            if (_tcpMode == TcpMode.Client)
            {
                if (Connected) _tc.Close();
            }
            else
            {
                foreach (__listen__readSmtcp client in Connections)
                {
                    client.CloseConnect();
                }
            }
        }
        /// <summary>
        /// �رմӼ�������������
        /// </summary>
        /// <param name="clientID"></param>
        public void CloseConnect(int clientID)
        {
            __listen__readSmtcp reader = GetListenClient(clientID);
            try { if (reader != null)reader.CloseConnect(); }
            catch { }
        }

        private void ReadDataFromListen()
        {
            _listenWork = true;
            while (_listenWork)
            {
                try
                {
                    TcpClient _tc = _tl.AcceptTcpClient();
                    __listen__readSmtcp reader = new __listen__readSmtcp(this, ref _tc, _protocol);
                    lock (_tcp_Reader_Array)
                        _tcp_Reader_Array.Add(reader);
                    Threads.ExecAsync(reader.Start);
                }
                catch { }
            }
        }

        /// <summary>
        /// ��ʹ�����ģʽ��ʱ���ȡ�жӺͻ����е��������ݡ�
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            if (!UseBlockRead) return null;
            byte[] result = null;
            try
            {
                int bit = 0;
                bool SmmpIsNull = true;
                while ((bit = _ns.ReadByte()) != -1 && SmmpIsNull)
                {
                    _blockReadBufferField = DataLoop((byte)bit, -1);
                    if (_blockReadBufferField != null)
                    {
                        SmmpIsNull = false;
                        result = _blockReadBufferField;
                        _blockReadBufferField = null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return result;
        }
        /// <summary>
        /// ��ʹ�����ģʽ��ʱ���ȡ�жӺͻ����е��������ݡ�
        /// </summary>
        /// <returns></returns>
        public byte[] Read(int timeoutSpan, int totalCount)
        {
            byte[] result = null;

            Threads.ExecAsync(_readByBlockInReadMethodHasTimeout);
            int count = 0;
            while (_blockReadBufferField == null && count < totalCount)
            {
                Thread.Sleep(timeoutSpan);
                count++;
            }
            if (_blockReadBufferField == null)
                readTimeout = true;
            else
            {
                readTimeout = false;
                result = _blockReadBufferField;
                _blockReadBufferField = null;
            }
            return result;
        }
        #region read_method_timeout
        bool readTimeout = false;
        byte[] _blockReadBufferField = null;
        void _readByBlockInReadMethodHasTimeout()
        {
            if (!UseBlockRead) return;

            if (readTimeout)
                return;
            try
            {
                int bit = 0;
                bool SmmpIsNull = true;
                while (SmmpIsNull && (bit = _ns.ReadByte()) != -1)
                {
                    _blockReadBufferField = DataLoop((byte)bit, -1);
                    if (_blockReadBufferField != null)
                        SmmpIsNull = false;
                }
            }
            catch
            {
                return;
            }
        }
        #endregion
        private void ReadDataFromConnectRemote()
        {
            try
            {
                int bit = 0;
                while (!UseBlockRead && (bit = _ns.ReadByte()) != -1) DataLoop((byte)bit, -1);
                onClose(_localTcpStruct, _remoteTcpStruct, -1);
            }
            catch { onClose(_localTcpStruct, _remoteTcpStruct, -1); }
        }
        internal byte[] DataLoop(byte data, int clientID)
        {
            _byteCache.Add(data);
            byte[] byteData = (byte[])_byteCache.ToArray(typeof(byte));

            ProtocalHeader pheader = _protocol.IsProtocolHeader(byteData);
            if (pheader.IsSmProtocol && !_netStatusInfo.Received)
            {
                if (pheader.PackectLength == 0)
                    _byteCache.Clear();
                else
                {
                    _netStatusInfo.PackectLength = pheader.PackectLength;
                    _netStatusInfo.Received = true;
                    _byteCache.Clear();
                }
            }
            else if (_netStatusInfo.Received)
            {
                byte[] b = _protocol.ReadFormSmProtocol(ref _netStatusInfo, byteData);
                if (b != null)
                {
                    onReceivedData(_localTcpStruct, _remoteTcpStruct, b, null);
                    _netStatusInfo.Received = false;
                    _byteCache.Clear();
                }
                return b;
            }
            return null;
        }


        private void async_th_listen()
        {
            StartListen(_localTcpStruct);
        }
        private void async_th_connect()
        {
            ConnectTo(_remoteTcpStruct);
        }

        internal void onClose(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, int clientID)
        {
            _remoteTcpStruct.IP = null;
            _remoteTcpStruct.Port = 0;
            if (_tcpMode == TcpMode.Client)
            {
                CloseConnect();
                _tcpMode = TcpMode.Unknown;
            }
            else if (_tcpMode == TcpMode.Server)
            {
                lock (_tcp_Reader_Array)
                {
                    foreach (object obj in _tcp_Reader_Array)
                    {
                        if (obj == null) continue;
                        if (((__listen__readSmtcp)obj).ClientID == clientID)
                        {
                            _tcp_Reader_Array.Remove(obj);
                            break;
                        }
                    }
                }
            }
            onTcpEvents(_localTcpStruct, _remoteTcpStruct, TcpOptionType.Disconnected, null);
        }
        private void onStopListen()
        {
            _remoteTcpStruct.IP = null;
            _remoteTcpStruct.Port = 0;

            _localTcpStruct.IP = null;
            _localTcpStruct.Port = 0;

            onTcpEvents(_localTcpStruct, _remoteTcpStruct, TcpOptionType.StopListen, null);
        }
        internal void onError(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpError error, Exception ex, TcpOptionType type, ITcpReader tcpReader)
        {
            if (OnError != null) OnError(localTcpInfo, remoteTcpInfo, error, ex, type, tcpReader);
        }

        internal void onTcpEvents(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpOptionType type, ITcpReader tcpReader)
        {
            switch (type)
            {
                case TcpOptionType.StartListen:
                    _listenning = true;
                    break;
                case TcpOptionType.StopListen:
                    _listenning = false;
                    _listenWork = false;
                    break;
                case TcpOptionType.ClientConnected:
                    break;
                case TcpOptionType.Connected:
                    break;
                case TcpOptionType.Disconnected:
                    if (_tcpMode == TcpMode.Client)
                        _tcpMode = TcpMode.Unknown;
                    break;
                case TcpOptionType.Connecting:
                    Thread.Sleep(0);
                    break;
                default:
                    break;
            }
            if (OnTcpEvents != null) OnTcpEvents(localTcpInfo, remoteTcpInfo, type, tcpReader);
        }
        internal void onConnectionEvent(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, ITcpReader tcpClient, int clientID)
        {
            if (OnConnectionEvent != null) OnConnectionEvent(localTcpInfo, remoteTcpInfo, tcpClient, clientID);
        }
        internal void onReceivedData(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, byte[] data, ITcpReader tcpReader)
        {
            if (OnReceivedData != null) OnReceivedData(localTcpInfo, remoteTcpInfo, data, tcpReader);
        }
        /// <summary>
        /// ��һ����׼EndPoint�ַ�����䵽TcpStruct����
        /// </summary>
        /// <param name="ipstring">��׼��EndPoint�ַ���</param>
        /// <param name="tstr">TcpStruct��ַ����</param>
        public static void FormatIPStringToTcpStruct(string ipstring, ref TcpStruct tstr)
        {
            string[] s = ipstring.Split(new string[] { ":" }, StringSplitOptions.None);
            tstr.IP = IPAddress.Parse(s[0]);
            tstr.Port = int.Parse(s[1]);
        }

        #region IDisposable ��Ա
        /// <summary>
        /// �ͷ�SmTcpʹ�õ�������Դ
        /// </summary>
        public void Dispose()
        {
            try
            {
                OnError = null;
                OnReceivedData = null;
                OnTcpEvents = null;
                OnConnectionEvent = null;
                if (Connected) CloseConnect();
                if (_listenning) StopListen(true);
                if (_tc != null)
                { if (_tc.Connected) _tc.Close(); }
                if (_tl != null)
                { try { _tl.Stop(); } catch { } }
                if (_ns != null) _ns.Dispose();
            }
            catch { }
        }

        #endregion

        #region ITcpReader ��Ա
        public TcpClient Client
        {
            get
            {
                return _tc;
            }
        }
        #endregion
    }
    /// <summary>
    /// ��SmTcp�еļ���ѭ����ȡ����
    /// </summary>
    public class __listen__readSmtcp : ITcpReader
    {
        SmTcp _tcp;
        TcpClient _tc;
        TcpStruct _remoteTcpStruct;
        NetworkStream _ns;
        SmPackectProtocol _protocol;
        StateFlag _objectFlag = new StateFlag();

        public event TcpReceiveDataEventHander OnReceivedData;
        int _clientID;
        ArrayList _byteCache = new ArrayList();
        ProtocalStatusInfo _netStatusInfo;


        public int ClientID
        {
            get { return _clientID; }
        }
        public TcpClient Client
        {
            get { return _tc; }
        }
        public StateFlag ObjectFlag
        {
            get { return _objectFlag; }
            set { _objectFlag = value; }
        }

        public __listen__readSmtcp(SmTcp tcp, ref TcpClient tc, SmPackectProtocol protocal)
        {
            _protocol = protocal;
            _clientID = new Random().Next(1, 1024000);
            _tc = tc;
            _tcp = tcp;
        }
        internal void Start()
        {
            try
            {
                _ns = _tc.GetStream();
                Tcp.FormatIPStringToTcpStruct(_tc.Client.RemoteEndPoint.ToString(), ref _remoteTcpStruct);

                _tcp.onConnectionEvent(_tcp._localTcpStruct, _remoteTcpStruct, this, ClientID);
                _tcp.onTcpEvents(_tcp._localTcpStruct, _remoteTcpStruct, TcpOptionType.ClientConnected, this);

                if (!_tc.Connected)
                {
                    close();
                    return;
                }

                int bit = 0;
                while ((bit = _ns.ReadByte()) != -1) DataLoop((byte)bit, _clientID);
                close();
            }
            catch { close(); }
        }
        private void DataLoop(byte data, int clientID)
        {
                _byteCache.Add(data);
                byte[] byteData = (byte[])_byteCache.ToArray(typeof(byte));

                ProtocalHeader pheader = _protocol.IsProtocolHeader(byteData);
                if (pheader.IsSmProtocol && !_netStatusInfo.Received)
                {
                    if (pheader.PackectLength == 0)
                        _byteCache.Clear();
                    else
                    {
                        _netStatusInfo.PackectLength = pheader.PackectLength;
                        _netStatusInfo.Received = true;
                        _byteCache.Clear();
                    }
                }
                else if (_netStatusInfo.Received)
                {
                    byte[] b = _protocol.ReadFormSmProtocol(ref _netStatusInfo, byteData);
                    if (b != null)
                    {
                        onReceivedData(_tcp._localTcpStruct, _remoteTcpStruct, b);
                        _netStatusInfo.Received = false;
                        _byteCache.Clear();
                    }
                }
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="byteData">��������</param>
        public void SendData(byte[] byteData)
        {
            if (byteData.Length == 0) return;
            if (!_tc.Connected) { _tcp.onError(_tcp._localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, this); return; }
            byte[] sendData = _protocol.MakeByteData(byteData);
            _ns.Write(sendData, 0, sendData.Length);
        }
        /// <summary>
        /// ��Է������ַ���
        /// </summary>
        /// <param name="s">�ַ�������</param>
        public void SendString(string s)
        {
            SendData(_tcp.DataEncoding.GetBytes(s));
        }
        private void onReceivedData(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, byte[] data)
        {
            if (OnReceivedData != null) OnReceivedData(localTcpInfo, remoteTcpInfo, data, this);
            _tcp.onReceivedData(_tcp._localTcpStruct, _remoteTcpStruct, data, this);
        }
        private void close()
        {
            OnReceivedData = null;
            if (_tc.Connected) _tc.Close();
            _tcp.onConnectionEvent(_tcp._localTcpStruct, _remoteTcpStruct, this, ClientID);
            _tcp.onClose(_tcp._localTcpStruct, _remoteTcpStruct, _clientID);
        }
        /// <summary>
        /// �رյ�ǰ��TcpClient����
        /// </summary>
        public void CloseConnect()
        {
            if (_tc != null && _tc.Connected)
            {
                if (_tc != null && _tc.Client != null && _tc.Connected)
                    _tc.Close();
            }
            if (_ns != null)
                _ns.Dispose();
        }
    }

    /// <summary>
    /// WEB�ͻ���ʵ�ù���
    /// </summary>
    public sealed class SmWebClient
    {
        WebClient _wclit;
        WebRequest _req;
        WebResponse _rep;
        private string _url;

        public SmWebClient()
        {
            InitClass();
        }
        private void InitClass()
        {
            _wclit = new WebClient();
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public void DownloadFile(string LocalPath)
        {
            _wclit.DownloadFile(_url, LocalPath);
        }

        public long GetRemoteFileSize()
        {
            _req = WebRequest.Create(_url);

            _rep = _req.GetResponse();
            long len = _rep.ContentLength;
            return len;
        }

        public string SendHttp(string url, string method, string datastr)
        {
            byte[] data = System.Text.Encoding.GetEncoding("GB2312").GetBytes(datastr);
            // ׼������... 
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "Post"; //Getor Post
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            // �������� 
            stream.Write(data, 0, data.Length);
            stream.Close();

            HttpWebResponse rep = (HttpWebResponse)req.GetResponse();
            Stream receiveStream = rep.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("GB2312");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);

            //Char[] read = new Char[256];
            //int count = readStream.Read(read, 0, 256);
            //StringBuilder sb = new StringBuilder("");
            //while (count > 0)
            //{
            //    String readstr = new String(read, 0, count);
            //    sb.Append(readstr);
            //    count = readStream.Read(read, 0, 256);
            //}

            //rep.Close();
            //readStream.Close();

            //return sb.ToString();
            return readStream.ReadToEnd();
        }
        public string SendHttp(string url)
        {
            WebRequest req = WebRequest.Create(url);
            string restring = "";
            WebResponse result = req.GetResponse();
            Stream ReceiveStream = result.GetResponseStream();
            StreamReader readerOfStream = new StreamReader(ReceiveStream, System.Text.Encoding.GetEncoding("GB2312"));
            restring = readerOfStream.ReadToEnd();
            ReceiveStream.Close();
            return restring;
        }
    }
}