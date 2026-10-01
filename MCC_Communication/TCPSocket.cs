using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using PaeLibGeneral;
using System.Diagnostics;


namespace MCC_Communication
{
    public enum ConnectState
    {
        ConnectFail,
        ConnectTimeOut,
        ConfigIsNull,
        ReceiveSuccessful
    }

    public class TCPSocket
    {
        private Socket m_ClientSocket = null;
        private SocketConfig m_SocketConfig = null;
        private string m_ReceiveData = string.Empty;
        public TCPSocket(SocketConfig config)
        {
            m_SocketConfig = config;
        }
        public string ReceiveData
        {
            get { return m_ReceiveData; }
        }

        public bool IsConnected
        {
            get { return m_ClientSocket.Connected; }
        }

        public bool Connect()
        {
            try
            {
                if (this.m_SocketConfig == null)
                {
                    return false;
                }
                IPAddress ip = IPAddress.Parse(m_SocketConfig.IPaddress);
                IPEndPoint server = new IPEndPoint(ip, m_SocketConfig.Port);
                m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_ClientSocket.Connect(server);
            }
            catch (Exception ex)
            {
                JLogger.LogDebug("ServerProtocol", "TCPSocket Connect function error = " + ex.Message, true);
                return false;
            }
            return this.IsConnected;
        }
        public bool Disconnect()
        {
            try
            {
                if (this.m_SocketConfig == null)
                {
                    return false;
                }
                if (m_ClientSocket != null)
                {
                    m_ClientSocket.Close();
                    m_ClientSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                JLogger.LogDebug("ServerProtocol", "TCPSocket Disconnect function error = " + ex.Message, true);
                return false;
            }
            return true;
        }


        public ConnectState SendToHost(byte[] sendData, int receiveLength)
        {
            if (this.m_SocketConfig == null)
            {
                return ConnectState.ConfigIsNull;
            }
            if (!this.IsConnected)
            {
                return ConnectState.ConnectFail;
            }

            while (true)
            {
                SocketError error = new SocketError();

                m_ClientSocket.Send(sendData, 0, sendData.Length, SocketFlags.None, out error);
                if (error == SocketError.Success)
                {
                    break;
                }

            }

            return ReceiveFormHost(receiveLength);
        }

        private ConnectState ReceiveFormHost(int receiveLength)
        {
            byte[] btmp = new byte[2048];
            int dfmessagelength = 62;
            int msglength = 0;
            m_ReceiveData = string.Empty;
            if (!this.IsConnected)
            {
                return ConnectState.ConnectFail;
            }


            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_SetSocketOption Start)", true);
                m_ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_SetSocketOption End)", true);
                while (true)
                {

                    //socket.Receive(btmp);
                    int len = m_ClientSocket.Receive(btmp);
                    m_ReceiveData += System.Text.Encoding.Default.GetString(btmp, 0, len);
                    if (m_ReceiveData.Length >= dfmessagelength)
                    {
                        msglength = int.Parse(m_ReceiveData.Substring(0, 8));
                        if (m_ReceiveData.Length >= dfmessagelength + msglength)
                        {
                            break;
                        }
                    }
                    if (sw.Elapsed.TotalSeconds > 3)
                    {
                        JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_TotalSeconds Timout)", true);
                        return ConnectState.ConnectTimeOut;//timeout
                    }

                }
            }
            catch (Exception ex)
            {
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_Exception " + ex.ToString() + ")", true);
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_m_ReceiveData = " + m_ReceiveData + ")", true);
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_m_dfmessagelength = " + dfmessagelength.ToString() + ")", true);
                //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_m_msglength = " + msglength.ToString() + ")", true);
                return ConnectState.ConnectTimeOut;//timeout
            }
            finally
            {
                if (m_ClientSocket != null)
                {
                    m_ClientSocket.Close();
                    m_ClientSocket.Dispose();
                }

            }
            return ConnectState.ReceiveSuccessful;
        }
    }
    public class SocketConfig
    {
        private string m_ipAddress = null;
        private int m_port = 0;
        public SocketConfig(string ipAddress, int port)
        {
            m_ipAddress = ipAddress;
            m_port = port;
        }
        public string IPaddress
        {
            get { return m_ipAddress; }
            set
            {
                m_ipAddress = value;
            }
        }
        public int Port
        {
            get { return m_port; }
            set
            {
                m_port = value;
            }
        }
    }
}
