using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PaeLibGeneral;
using System.Diagnostics;
using System.Linq;

namespace MCC_Communication
{
    public class MCC_BMP_Host
    {
        private int m_serverPort = 5555;
        private string m_serverIP = "127.0.0.1";
        private TcpListener m_listener;
        private bool m_isRunning = false;

        private string UpdateMsg = string.Empty;
        private bool m_Refresh = false;
        public bool bRefresh
        {
            get { return m_Refresh; }
        }

        public string UpdateText()
        {
            string sRet = string.Empty;
            if (m_Refresh)
            {
                sRet = UpdateMsg;
                UpdateMsg = string.Empty;
                m_Refresh = false;
            }
            return sRet;
        }

        private void AddMsg(string str)
        {
            UpdateMsg += str.TrimEnd('\0') + "\r\n";
            m_Refresh = true;
        }

        public MCC_BMP_Host()
        {
            ReadSetup();
        }

        private bool ReadSetup()
        {
            bool bRet = false;
            string _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System", "Setup.ini");
            if (File.Exists(_fullPath))
            {
                using (TINIFile ini = new TINIFile(_fullPath))
                {
                    m_serverPort = ini.ReadInteger("MCCLink", "ServerPort", 5555);
                    m_serverIP = ini.ReadString("MCCLink", "MainAddress", "127.0.0.1");
                    bRet = true;
                }
            }
            return bRet;
        }

        //public void Start()
        private TcpClient client = null;
        private NetworkStream stream = null;
        public async Task Start()
        {
            m_listener = new TcpListener(IPAddress.Parse(m_serverIP), m_serverPort);
            m_listener.Start();
            m_isRunning = true;
            Console.WriteLine("Server started on " + m_serverIP + ":" + m_serverPort);
            AddMsg("Server started on " + m_serverIP + ":" + m_serverPort);

            while (m_isRunning)
            {
                try
                {
                    //TcpClient client = m_listener.AcceptTcpClient();
                    client = await m_listener.AcceptTcpClientAsync();
                    Thread clientThread = new Thread(HandleClient);
                    //clientThread.Start(client);
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    AddMsg("Error: " + ex.Message);
                }
            }
        }

        public void Stop()
        {
            m_isRunning = false;
            m_listener.Stop();
        }

        public async void SendData(byte[] data)
        {
            string val = Encoding.Default.GetString(data, 0, data.Length);
            await SendData(val);
        }
        private async Task SendData(string data)
        {
            if (!m_isRunning)
            {
                AddMsg("Server not running!");
                return;
            }

            try
            {
                // 建立 TcpClient 並連線到伺服器
                try
                {
                    stream = client.GetStream();
                    // 將資料轉換為 byte 陣列
                    byte[] dataBytes = Encoding.Default.GetBytes(data);

                    // 發送資料
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length);
                    AddMsg(string.Format("Sent：{0}", data));
                }
                finally
                {
                    if (stream != null)
                    {
                        stream = null;
                    }
                }
                //using (TcpClient client = await m_listener.AcceptTcpClientAsync())
                //{
                //    //TcpClient client = await m_listener.AcceptTcpClientAsync();
                //    //await client.ConnectAsync(IPAddress.Parse(m_serverIP), m_serverPort);

                //    // 取得 NetworkStream
                //    using (NetworkStream stream = client.GetStream())
                //    {
                //        // 將資料轉換為 byte 陣列
                //        byte[] dataBytes = Encoding.Default.GetBytes(data);

                //        // 發送資料
                //        await stream.WriteAsync(dataBytes, 0, dataBytes.Length);
                //        AddMsg(string.Format("Sent：{0}", data));
                //    }
                //}
            }
            catch (Exception ex)
            {
                //AddMsg($"發送資料時發生錯誤：{ex.Message}");
            }
        }

        //private async void HandleClient(object obj)
        //private void HandleClient(object obj)
        private async void HandleClient()
        {
            //TcpClient client = (TcpClient)obj;
            //NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            // 設置接收超時為 5 秒（5000 毫秒）
            client.ReceiveTimeout = 5000;
            // 啟用 KeepAlive
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //try
            //{
            //    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            //    {
            //        string request = Encoding.Default.GetString(buffer, 0, bytesRead);
            //        Console.WriteLine("Received: " + request);
            //        AddMsg("Received: " + request);

            //        // 處理請求並生成回應
            //        string response = ProcessRequest(request);
            //        byte[] responseBytes = Encoding.Default.GetBytes(response);

            //        // 發送回應
            //        //stream.Write(responseBytes, 0, responseBytes.Length);
            //        //stream.Flush();
            //        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            //        await stream.FlushAsync();
            //        Console.WriteLine("Sent: " + response);
            //        AddMsg("Sent: " + response);
            //    }
            //}
            try
            {
                while (true)
                {
                    // 使用 ReadAsync 進行非同步讀取
                    stream = client.GetStream();
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // 客戶端關閉連線
                        break;
                    }

                    string request = Encoding.Default.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + request);
                    AddMsg("Received: " + request);

                    // 處理請求並生成回應
                    string response = ProcessRequest(request);
                    if (response != "NULL")
                    {
                        byte[] responseBytes = Encoding.Default.GetBytes(response);

                        // 發送回應
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        await stream.FlushAsync();
                        Console.WriteLine("Sent: " + response);
                        AddMsg("Sent: " + response);
                    }
                    if (stream != null)
                    {
                        stream = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                //AddMsg("Error: " + ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream = null;
                //client.Close();
            }
        }
        
        private string ProcessRequest(string request)
        {
            int msglength = 0;
            int dfmessagelength = 62;
            byte[] byteArr = null;
            byteArr = System.Text.Encoding.Default.GetBytes(request);
            MCC_BibMap m_BibMap = new MCC_BibMap();
            string sType = request.Substring(8, 4).Trim();
            if (sType == "ACK") sType = "ACKMessage";
            if (sType == "NCK") sType = "NCKMessage";
            PackType Type = (PackType)Enum.Parse(typeof(PackType), sType);
            string mode = request.Substring(62, 20).Trim();
            
            m_BibMap.PackFormat(mode, Type);
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                while (true)
                {
                    if (request.Length >= dfmessagelength)
                    {
                        msglength = int.Parse(request.Substring(0, 8));
                        if (request.Length >= dfmessagelength + msglength)
                        {
                            break;
                        }
                    }
                    if (sw.Elapsed.TotalSeconds > 3)
                    {
                        //JLogger.LogDebug("ServerProtocol", "(ReceiveFormHost_TotalSeconds Timout)", true);
                        AddMsg("(ReceiveFormHost_TotalSeconds Timout)");
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                //JLogger.LogDebug("ServerProtocol", ex.Message, true);
                AddMsg("ERROR: " + ex.Message);
            }
            Array.Copy(byteArr, 0, m_BibMap.bytedata1, 0, m_BibMap.bytedata1.Length);
            //AddMsg("Receive:" + m_Socket.ReceiveData);
            //byteArr = System.Text.Encoding.Default.GetBytes("EEXT  E    568      568Mf2220231214181540123456789012         953960      QA-272-0                     0                      f22                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                ");
            int ret = m_BibMap.PackAnalysis(mode, PackType.DFMessage, m_BibMap.bytedata1);
            PackType mytype; 
            if (ret == 1)
            {
                switch (msglength)
                {
                    case 20:
                        mytype = PackType.ACKMessage;
                        break;
                    case 280:
                        mytype = PackType.NCKMessage;
                        //mytype = Type;
                        break;
                    default:
                        mytype = Type;
                        break;
                }
                m_BibMap.bytedata2 = new byte[msglength];
                Array.Copy(byteArr, m_BibMap.bytedata1.Length, m_BibMap.bytedata2, 0, msglength);
                //ret = m_BibMap.PackAnalysis(mode, mytype, m_BibMap.bytedata2);
                ret = m_BibMap.PackResult(mode, mytype);
                if (ret == 1)
                {
                    switch (mytype)
                    {
                        case PackType.ISB:
                        case PackType.EJB:
                        case PackType.STG:
                            PackType type = PackType.ACKMessage;
                            if (mode != "Insert" && mode != "Load" && mode != "Unload" &&
                                mode != "Exchange" && mode != "Remove" && mode != "Swap")
                            {
                                type = PackType.NCKMessage;
                            }
                            m_BibMap.PackFormat(mode, type);
                            byte[] Data = m_BibMap.bytedata1.Concat(m_BibMap.bytedata2).ToArray();
                            return System.Text.Encoding.Default.GetString(Data);
                    }
                }
            }
            return "NULL";
        }
    }
}
