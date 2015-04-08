using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Silmoon.Memory;
using Silmoon.Net.SmProtocol;
using Silmoon.Threading;
using System.Runtime.InteropServices;

namespace Silmoon.Net
{
    /// <summary>
    /// ����TCP�����ṩSMMP(������ϢЭ��)Э�����͵�ͨѶ���ܣ�
    /// </summary>
    public class Smmp : IDisposable
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
        public event SmmpOptionEventHander OnTcpEvents;
        /// <summary>
        /// �����յ�����
        /// </summary>
        public event SmmpReceiveDataEventHander OnReceivedData;
        /// <summary>
        /// �����������ʱ�򣬶��������첽�쳣����
        /// </summary>
        public event SmmpOnErrorEventHander OnError;
        /// <summary>
        /// �����������¼�ʱ��ָ������
        /// </summary>
        public event SmmpOnConnectionEventHander OnConnectionEvent;

        TcpClient _tc;
        TcpListener _tl;
        NetworkStream _ns;

        ArrayList _byteCache = new ArrayList();
        ArrayList _tcp_Reader_Array = new ArrayList();

        SmmpPackectProtocol _protocol = new SmmpPackectProtocol();
        SmmpProtocalHeader _savedHeaderInfo;

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
        public __listen__readSmmp[] Connections
        {
            get { return (__listen__readSmmp[])_tcp_Reader_Array.ToArray(typeof(__listen__readSmmp)); }
        }

        /// <summary>
        /// ����SMTCP��ʵ��
        /// </summary>
        public Smmp()
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
        /// �첽���ӵ�һ��֧��SMMPЭ��ļ�����˿�
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
        /// ���ӵ�Զ��SMMPЭ������
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
        /// ��Է���������
        /// </summary>
        /// <param name="stringData">�ַ�������</param>
        public void SendData(string stringData)
        {
            SendData(stringData, -1);
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
        /// <param name="packet">SMMP���ݰ�</param>
        public void SendData(SmmpPacket packet)
        {
            SendData(packet, -1);
        }


        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="byteData">��������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendData(string stringData, int clientID)
        {
            SendData(DataEncoding.GetBytes(stringData), clientID);
        }
        /// <summary>
        /// ��Է��������ݣ����ڿͻ�������
        /// </summary>
        /// <param name="byteData">��������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendData(byte[] byteData, int clientID)
        {
            SmmpPacket packet = new SmmpPacket(new Random().Next(1, 999999), -1);
            packet.MakeByteData(byteData);
            SendData(packet, clientID);
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="byteData">��������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendData(SmmpPacket packet, int clientID)
        {
            if (_tcpMode == TcpMode.Client)
            {
                if (!Connected) { onError(_localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, null); return; }
                byte[] bytePackect = packet.StartMakeup();
                _ns.Write(bytePackect, 0, bytePackect.Length);
            }
            else if (_tcpMode == TcpMode.Server)
            {
                __listen__readSmmp pclinet = GetListenClient(clientID);
                if (pclinet != null)
                    pclinet.SendData(packet);
                else
                    onError(_localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, pclinet);
            }
            else
                onError(_localTcpStruct, _remoteTcpStruct, TcpError.UnknownModeOrNotConnected, null, TcpOptionType.SendData, null);
        }

        /// <summary>
        /// ��ȡ�����Լ����Ĵ�������
        /// </summary>
        /// <param name="clientID">����ID</param>
        /// <returns></returns>
        public __listen__readSmmp GetListenClient(int clientID)
        {
            foreach (object obj in _tcp_Reader_Array)
            {
                if (obj != null && ((__listen__readSmmp)obj).ClientID == clientID)
                    return (__listen__readSmmp)obj;
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
                foreach (__listen__readSmmp client in Connections)
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
            __listen__readSmmp reader = GetListenClient(clientID);
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
                    __listen__readSmmp reader = new __listen__readSmmp(this, ref _tc);
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
        public SmmpPacket Read()
        {
            if (!UseBlockRead) return null;
            SmmpPacket result = null;
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
        public SmmpPacket Read(int timeoutSpan, int totalCount)
        {
            SmmpPacket result = null;

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
        SmmpPacket _blockReadBufferField = null;
        void _readByBlockInReadMethodHasTimeout()
        {
            if (!UseBlockRead) return;

            if (readTimeout)
                return;
            try
            {
                int bit = 0;
                bool SmmpIsNull = true;
                while ((bit = _ns.ReadByte()) != -1 && SmmpIsNull)
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
        internal SmmpPacket DataLoop(byte data, int clientID)
        {
            _byteCache.Add(data);
            string s = DataEncoding.GetString((byte[])_byteCache.ToArray(typeof(byte)));
            if (!_protocol.Received)
            {
                SmmpProtocalHeader pheader = _protocol.IsProtocolHeader(ref _byteCache);
                if (pheader != null)
                {
                    _savedHeaderInfo = pheader;
                    _protocol.Received = true;
                }
            }
            else if (_protocol.Received)
            {
                SmmpPacket packet = _protocol.ReadFormSmProtocol(ref _byteCache, ref _savedHeaderInfo);
                if (packet != null)
                {
                    onReceivedData(_localTcpStruct, _remoteTcpStruct, packet, null);
                    _protocol.Received = false;
                    _byteCache.Clear();
                }
                return packet;
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
                        if (((__listen__readSmmp)obj).ClientID == clientID)
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
        internal void onError(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpError error, Exception ex, TcpOptionType type, __listen__readSmmp tcpReader)
        {
            if (OnError != null) OnError(localTcpInfo, remoteTcpInfo, error, ex, type, tcpReader);
        }

        internal void onTcpEvents(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpOptionType type, __listen__readSmmp tcpReader)
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
        internal void onConnectionEvent(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, __listen__readSmmp tcpClient, int clientID)
        {
            if (OnConnectionEvent != null) OnConnectionEvent(localTcpInfo, remoteTcpInfo, tcpClient, clientID);
        }
        internal void onReceivedData(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, SmmpPacket packet, __listen__readSmmp tcpReader)
        {
            if (OnReceivedData != null) OnReceivedData(localTcpInfo, remoteTcpInfo, packet, tcpReader);
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
    /// ��Smmp�еļ���ѭ����ȡ����
    /// </summary>
    public class __listen__readSmmp
    {
        Smmp _tcp;
        TcpClient _tc;
        TcpStruct _remoteTcpStruct;
        NetworkStream _ns;
        SmmpPackectProtocol _protocol = new SmmpPackectProtocol();
        StateFlag _objectFlag = new StateFlag();

        public event SmmpReceiveDataEventHander OnReceivedData;
        int _clientID;
        ArrayList _byteCache = new ArrayList();


        SmmpProtocalHeader _savedHeaderInfo;


        public int ClientID
        {
            get { return _clientID; }
            set { _clientID = value; }
        }
        public TcpClient Client
        {
            get { return _tc; }
            set { _tc = value; }
        }
        public StateFlag ObjectFlag
        {
            get { return _objectFlag; }
            set { _objectFlag = value; }
        }

        public __listen__readSmmp(Smmp tcp, ref TcpClient tc)
        {
            _clientID = new Random().Next(1, 1024000);
            _tc = tc;
            _tcp = tcp;
        }
        internal void Start()
        {
            int count = 1;

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
                while ((bit = _ns.ReadByte()) != -1)
                {
                    DataLoop((byte)bit, _clientID);
                    count++;
                }
                close();
            }
            catch
            { close(); }
        }
        private unsafe void DataLoop(byte data, int clientID)
        {
            _byteCache.Add(data);

            if (!_protocol.Received)
            {
                _savedHeaderInfo = _protocol.IsProtocolHeader(ref _byteCache);
                if (_savedHeaderInfo != null)
                {
                    _protocol.Received = true;
                }
                else _protocol.Received = false;
            }
            else if (_protocol.Received)
            {
                SmmpPacket packet = _protocol.ReadFormSmProtocol(ref _byteCache, ref _savedHeaderInfo);
                if (packet != null)
                {
                    onReceivedData(_tcp._localTcpStruct, _remoteTcpStruct, packet, (byte[])_byteCache.ToArray(typeof(byte)));
                    _protocol.Received = false;
                    _byteCache.Clear();
                }
            }
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <SmmpPacket name="byteData">��������</param>
        public void SendData(SmmpPacket packet)
        {
            if (!_tc.Connected) { _tcp.onError(_tcp._localTcpStruct, _remoteTcpStruct, TcpError.TcpClientNotConnected, null, TcpOptionType.SendData, this); return; }
            byte[] bytePackect = packet.StartMakeup();
            _ns.Write(bytePackect, 0, bytePackect.Length);
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="stringData">��������</param>
        public void SendData(string stringData)
        {
            SendData(_tcp.DataEncoding.GetBytes(stringData));
        }
        /// <summary>
        /// ��Է���������
        /// </summary>
        /// <param name="byteData">��������</param>
        /// <param name="clientID">���ӱ�ʶ</param>
        public void SendData(byte[] byteData)
        {
            SmmpPacket packet = new SmmpPacket(new Random().Next(1, 999999), -1);
            packet.MakeByteData(byteData);
            SendData(packet);
        }

        private void onReceivedData(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, SmmpPacket packet, byte[] rawData)
        {
            if (OnReceivedData != null) OnReceivedData(localTcpInfo, remoteTcpInfo, packet, this);
            _tcp.onReceivedData(_tcp._localTcpStruct, _remoteTcpStruct, packet, this);
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
    /// Smmp���ݱ�
    /// </summary>
    public class SmmpPacket
    {
        /// <summary>
        /// ��ϢID
        /// </summary>
        public int MessageID = 0;
        /// <summary>
        /// ��ӦID
        /// </summary>
        public int ResponseID = 0;
        /// <summary>
        /// ��չ��Ϣ
        /// </summary>
        public NameValueCollection Messages;

        public byte[] ContentBuffer;
        public SmmpPacket(bool initMessages = false)
        {
            if (initMessages) Messages = new NameValueCollection();
        }
        public SmmpPacket(int messageID = -1, int responseID = -1, bool initMessages = false)
        {
            if (messageID == -1)
                messageID = new Random().Next(0, 99999);
            if (responseID == -1)
                responseID = new Random().Next(0, 99999);

            MessageID = messageID;
            ResponseID = responseID;
            if (initMessages) Messages = new NameValueCollection();
        }
        public byte[] MakeByteData(int messageID, int responseID)
        {
            MessageID = messageID;
            ResponseID = responseID;
            Messages = new NameValueCollection();
            return StartMakeup();
        }
        public byte[] MakeByteData(int messageID, int responseID, NameValueCollection messages)
        {
            MessageID = messageID;
            ResponseID = responseID;
            Messages = messages;
            return StartMakeup();
        }
        public byte[] MakeByteData(byte[] data)
        {
            ContentBuffer = data;
            Messages = new NameValueCollection();
            return StartMakeup();
        }
        public byte[] MakeByteData(int messageID, byte[] data)
        {
            MessageID = messageID;
            ResponseID = 0;
            ContentBuffer = data;
            Messages = new NameValueCollection();
            return StartMakeup();
        }
        public byte[] MakeByteData(int messageID, int responseID, byte[] data)
        {
            MessageID = messageID;
            ResponseID = responseID;
            ContentBuffer = data;
            Messages = new NameValueCollection();
            return StartMakeup();
        }
        public byte[] MakeByteData(int messageID, int responseID, NameValueCollection messages, byte[] data)
        {
            MessageID = messageID;
            ResponseID = responseID;
            Messages = messages;
            ContentBuffer = data;
            return StartMakeup();
        }

        public byte[] StartMakeup()
        {

            if (Messages == null) Messages = new NameValueCollection();
            if (ContentBuffer != null)
            {
                if (Messages["ContentLength"] == null)
                {
                    Messages.Add("ContentLength", ContentBuffer.Length.ToString());
                }
            }
            else Messages.Add("ContentLength", "0");


            string messageStringSave = "";
            int MessagesBytes = 0;
            foreach (string msgName in Messages)
            {
                messageStringSave += msgName + ":" + Messages[msgName] + "\r\n";
                MessagesBytes += msgName.Length + Messages[msgName].Length + 3;
            }

            ArrayList Data = new ArrayList();
            byte[] headerBytes = Encoding.Default.GetBytes("SMMP\r\n" + MessageID + "\r\n" + ResponseID + "\r\n" + Encoding.Default.GetBytes(messageStringSave).Length + "\r\n");

            for (int i = 0; i < headerBytes.Length; i++)
                Data.Add(headerBytes[i]);

            byte[] messageByte = Encoding.Default.GetBytes(messageStringSave);

            foreach (byte b in messageByte)
                Data.Add(b);

            if (ContentBuffer != null)
            {
                foreach (byte b in ContentBuffer)
                    Data.Add(b);
            }

            return (byte[])Data.ToArray(typeof(byte));
        }

        /// <summary>
        /// �ӽ��ܵ���SMMP���ݰ������µ���Ӧ���ݰ���
        /// </summary>
        /// <param name="fromRecv">���ܵ������ݰ�</param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static SmmpPacket CreateNew(SmmpPacket fromRecv, int ID = -1)
        {
            if (ID == -1) ID = new Random().Next(0, 99999);
            SmmpPacket packet = new SmmpPacket(ID, fromRecv.MessageID, true);
            return packet;
        }
        public byte[] RawData = null;
    }

    /// <summary>
    /// SMЭ�鴦�����
    /// </summary>
    public class SmmpPackectProtocol
    {
        int contentLength = 0;
        SmmpPacket packet;
        bool MessageRead = false;

        public bool Received = false;
        /// <summary>
        /// ��ʵ��Э�鴦�����
        /// </summary>
        public SmmpPackectProtocol()
        {

        }
        /// <summary>
        /// ��ȡSMЭ��ͷ�ṹ����
        /// </summary>
        /// <param name="packetBuffer">����</param>
        /// <returns></returns>
        public SmmpProtocalHeader IsProtocolHeader(ref ArrayList packetBuffer)
        {
            SmmpProtocalHeader HeaderInfo = new SmmpProtocalHeader();
            HeaderInfo.IsSmProtocol = false;


            string stringData = Encoding.Default.GetString((byte[])packetBuffer.ToArray(typeof(byte)));
            if (stringData.Length < 6)
                return null;

            int startC = stringData.IndexOf("SMMP\r\n");
            if (startC == -1)
            {
                if (packetBuffer.Count > 1024) packetBuffer.Clear();
                return null;
            }

            stringData = stringData.Substring(startC, stringData.Length - startC);

            string[] headerLine = stringData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (headerLine.Length < 5) return null;
            else
            {
                if (headerLine[0] != "SMMP")
                {
                    packetBuffer.Clear();
                    return null;
                }
                if (!int.TryParse(headerLine[1], out HeaderInfo.MessageID))
                {
                    packetBuffer.Clear();
                    return null;
                }
                if (!int.TryParse(headerLine[2], out HeaderInfo.ResponseID))
                {
                    packetBuffer.Clear();
                    return null;
                }
                if (!int.TryParse(headerLine[3], out HeaderInfo.MessagesBytes))
                {
                    packetBuffer.Clear();
                    return null;
                }
                HeaderInfo.IsSmProtocol = true;
                packetBuffer.Clear();
            }
            return HeaderInfo;
        }
        /// <summary>
        /// ����SMЭ��ͷ�����״̬��ȡ����
        /// </summary>
        /// <param name="packetBuffer">��״̬�����е�����</param>
        /// <param name="headerInfo">Э��ͷ��Ϣ</param>
        /// <returns></returns>
        public SmmpPacket ReadFormSmProtocol(ref ArrayList packetBuffer, ref SmmpProtocalHeader headerInfo)
        {
            byte[] bytes = (byte[])packetBuffer.ToArray(typeof(byte));
            if (bytes.Length >= headerInfo.MessagesBytes && contentLength == 0)
            {
                byte[] headerMessageBytes = new byte[headerInfo.MessagesBytes];
                Memory.Memory.MemCpy(ref headerMessageBytes, ref bytes);

                string[] messageLines = Encoding.Default.GetString(headerMessageBytes).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                packet = new SmmpPacket(headerInfo.MessageID, headerInfo.ResponseID);
                packet.Messages = new NameValueCollection();

                foreach (string messageLine in messageLines)
                {
                    string[] messageSplited = messageLine.Split(new string[] { ":" }, 2, StringSplitOptions.None);
                    if (messageSplited.Length == 2)
                        packet.Messages.Add(messageSplited[0], messageSplited[1]);
                }
                MessageRead = true;
                packetBuffer.Clear();
                if (packet.Messages.Count == 0) return null;
                if (int.TryParse(packet.Messages["ContentLength"], out contentLength))
                {
                    if (contentLength != 0)
                        return null;
                }
                Received = false;
                packet.RawData = bytes;
                return packet;
            }
            else if (bytes.Length >= contentLength && contentLength > 0)
            {
                if (packet == null) return null;
                packet.ContentBuffer = new byte[contentLength];
                for (int i = 0; i < contentLength; i++)
                    packet.ContentBuffer[i] = bytes[i];

                packetBuffer.Clear();
                Received = false;
                contentLength = 0;
                packet.RawData = bytes;
                return packet;
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
    public class SmmpProtocalHeader
    {
        public bool IsSmProtocol;
        public int MessageID;
        public int ResponseID;
        public int MessagesBytes;
    }
    /// <summary>
    /// TCP�¼�ί��
    /// </summary>
    /// <param name="localTcpInfo">����TCP�ṹ</param>
    /// <param name="remoteTcpInfo">Զ��TCP�ṹ</param>
    /// <param name="type">��������</param>
    /// <param name="clientID">���ӱ�ʶ</param>
    public delegate void SmmpOptionEventHander(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpOptionType type, __listen__readSmmp tcpReader);
    /// <summary>
    /// ���ܵ����ݣ����������ί��
    /// </summary>
    /// <param name="localTcpInfo">����TCP�ṹ</param>
    /// <param name="remoteTcpInfo">Զ��TCP�ṹ</param>
    /// <param name="data">���������������</param>
    /// <param name="clientID">���ӱ�ʶ</param>
    public delegate void SmmpReceiveDataEventHander(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, SmmpPacket packet, __listen__readSmmp tcpReader);
    /// <summary>
    /// ��Tcp���������ʱ�������첽���������쳣�������
    /// </summary>
    /// <param name="localTcpInfo">����TCP�ṹ</param>
    /// <param name="remoteTcpInfo">Զ��TCP�ṹ</param>
    /// <param name="Error">��������</param>
    /// <param name="Ex">�ϲ�����Ĵ���</param>
    /// <param name="type">��������</param>
    /// <param name="clientID">���ӱ�ʶ</param>
    public delegate void SmmpOnErrorEventHander(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, TcpError Error, Exception Ex, TcpOptionType type, __listen__readSmmp tcpReader);
    /// <summary>
    /// ��Tcp����ʱ�䷢��ʱ�Ĵ������
    /// </summary>
    /// <param name="localTcpInfo">����TCP�ṹ</param>
    /// <param name="remoteTcpInfo">Զ��TCP�ṹ</param>
    /// <param name="tcpClient">�����¼���TcpClientʵ��</param>
    /// <param name="clientID">ClientID</param>
    public delegate void SmmpOnConnectionEventHander(TcpStruct localTcpInfo, TcpStruct remoteTcpInfo, __listen__readSmmp tcpReader, int clientID);

}