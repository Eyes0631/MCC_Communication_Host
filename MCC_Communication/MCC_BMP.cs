using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaeLibGeneral;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Remoting;

namespace MCC_Communication
{
        public enum BoardMapDir
    {
        TBRL,
        TBLR,
        BTRL,
        BTLR,
        RLTB,
        RLBT,
        LRTB,
        LRBT
    }
    public enum PackType
    {
        ISB = 1,
        EJB = 2,
        STG = 3,
        SDP = 4,
        ETS = 5,
        ACKMessage = 6,
        NCKMessage = 7,
        DFMessage = 8,
    }
        class ProvBibMapStruct
    {
        //測試程式名稱 (8 bytes)
        public byte[] ProgramFileName { get; set; }
        //條碼 (4 bytes)
        public int BarCode { get; set; }
        //插槽編號 (1 byte)
        public byte BibSlotNo { get; set; }
        //產品名稱 (30 bytes)
        public byte[] ProductName { get; set; }
        //批號 (15 bytes)
        public byte[] LotNo { get; set; }
        //子批號 (4 bytes)
        public int DivideLotNo { get; set; }
        //最後更新日期 (8 bytes)
        public byte[] ModifiedDate { get; set; }
        //最後更新時間 (8 bytes)
        public byte[] ModifiedTime { get; set; }
        //板號 (8 bytes)
        public byte[] BibID { get; set; }
        //行數 (2 bytes)
        public short ColSocketCount { get; set; }
        //列數 (2 bytes)
        public short RowSocketCount { get; set; }
        //Map (1024 duts, 2 bytes 代表一個 dut, 所以共 2048 bytes)
        public char[] Map { get; set; }

        public ProvBibMapStruct()
        {
            ProgramFileName = new byte[8];
            ProductName = new byte[30];
            LotNo = new byte[100];
            ModifiedDate = new byte[8];
            ModifiedTime = new byte[8];
            BibID = new byte[20];
            Map = new char[512];
        }
        public void UpdateTime()
        {
            DateTime Date = DateTime.Now;
            byte[] dataArray = Encoding.Default.GetBytes(Date.ToString("yy/MM/dd"));    //使用系統預設編碼轉換
            int dataArrayLength = Math.Min(this.ModifiedDate.Length, dataArray.Length);
            Array.Copy(dataArray, this.ModifiedDate, dataArrayLength);
            dataArray = Encoding.Default.GetBytes(Date.ToString("hh:mm:ss"));
            dataArrayLength = Math.Min(this.ModifiedTime.Length, dataArray.Length);
            Array.Copy(dataArray, this.ModifiedTime, dataArrayLength);
        }
    }
    class MCC_BibMap
    {
        private bool m_PackParser = false;
        private string m_ErrMsg = string.Empty;
        private int m_slot = 0;
        private int m_station = 0;
        private string m_mode = string.Empty;
        private string m_lotno = string.Empty;
        private string m_type = string.Empty;
        private string m_program = string.Empty;
        private MCCDataStruct.DFMessage messageformat_parser = null;
        private MCCDataStruct.DFMessage messageformat = null;
        private MCCDataStruct.ACKMessage ACKM = null;
        private MCCDataStruct.NCKMessage NCKM = null;
        private MCCDataStruct.ISB isbformat = null;
        private MCCDataStruct.ISB isbformat_parser = null;
        private MCCDataStruct.EJB ejbformat = null;
        private MCCDataStruct.EJB ejbformat_parser = null;
        private MCCDataStruct.STG stgformat = null;
        private MCCDataStruct.STG stgformat_parser = null;
        private MCCDataStruct.SDP sdpformat = null;
        private MCCDataStruct.SDP sdpformat_parser = null;
        private MCCDataStruct.ETS etsformat = null; 
        private MCCDataStruct.ETS etsformat_parser = null;

        private MemoryStream stream = null;
        private BinaryReader reader = null;
        private BinaryWriter writer = null;

        private const int COL_COULTS = 32;
        private const  int ROW_COULTS = 32;
        private ProvBibMapStruct m_BibMap = null;
        public MCC_BibMap()
        {
            ReadSetup();
            PackInitial();
            m_BibMap = new ProvBibMapStruct();
        }

        ~MCC_BibMap()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (ACKM != null)
            {
                ACKM = null;
            }
            if (NCKM != null)
            {
                NCKM = null;
            }
            if (isbformat != null)
            {
                isbformat = null;
            }
            if (isbformat_parser != null)
            {
                isbformat_parser = null;
            }
            if (ejbformat != null)
            {
                ejbformat = null;
            }
            if (ejbformat_parser != null)
            {
                ejbformat_parser = null;
            }
            if (stgformat != null)
            {
                stgformat = null;
            }
            if (stgformat_parser != null)
            {
                stgformat_parser = null;
            }
            if (sdpformat != null)
            {
                sdpformat = null;
            }
            if (sdpformat_parser != null)
            {
                sdpformat_parser = null;
            }
            if (etsformat != null)
            {
                etsformat = null;
            }
            if (etsformat_parser != null)
            {
                etsformat_parser = null;
            }
            if (messageformat != null)
            {
                messageformat = null;
            }
            if (messageformat_parser != null)
            {
                messageformat_parser = null;
            }
            if (m_BibMap != null)
            {
                m_BibMap = null;
            }
        }

        private void PackInitial()
        {
            messageformat_parser = new MCCDataStruct.DFMessage();
            messageformat = new MCCDataStruct.DFMessage();
            ACKM = new MCCDataStruct.ACKMessage();
            NCKM = new MCCDataStruct.NCKMessage();
            isbformat = new MCCDataStruct.ISB();
            isbformat_parser = new MCCDataStruct.ISB();
            ejbformat = new MCCDataStruct.EJB();
            ejbformat_parser = new MCCDataStruct.EJB();
            stgformat = new MCCDataStruct.STG();
            stgformat_parser = new MCCDataStruct.STG();
            sdpformat = new MCCDataStruct.SDP();
            sdpformat_parser = new MCCDataStruct.SDP();
            etsformat = new MCCDataStruct.ETS();       
            etsformat_parser = new MCCDataStruct.ETS();     
        }

        private bool ReadSetup()
        {
            bool bRet = false;
            string _fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "System", "Setup.ini");
            if (File.Exists(_fullPath))
            {
                using (TINIFile ini = new TINIFile(_fullPath))
                {
                    
                    bRet = true;
                }
            }
            return bRet;
        }

        private void ACKFormat(string mode)
        {
            if (!m_PackParser)
            {
                ACKM.Reset(' ');
                ACKM.RunMode = mode.ToCharArray(0, mode.Length);
                writer.Write(ACKM.RunMode);
            }
            else
            {
                ACKM.Reset(' ');
                ACKM.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ACKM.RunMode.Length)).ToArray();
            }
        }

        private void NCKFormat(string mode)
        {
            if (!m_PackParser)
            {
                NCKM.Reset(' ');
                NCKM.RunMode = mode.ToCharArray(0, mode.Length);
                string code = "0001";
                string msg = "RunMode Setting Error";
                NCKM.ErrorCode = code.ToCharArray(0, code.Length);
                NCKM.ErrorMessage = msg.ToCharArray(0, msg.Length);
                writer.Write(NCKM.RunMode);
                writer.Write(NCKM.ErrorCode);
                writer.Write(NCKM.ErrorMessage);
            }
            else
            {
                NCKM.Reset(' ');
                NCKM.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(NCKM.RunMode.Length)).ToArray();
                NCKM.ErrorCode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(NCKM.ErrorCode.Length)).ToArray();
                NCKM.ErrorMessage = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(NCKM.ErrorMessage.Length)).ToArray();
            }
        }

        private void ISBFormat(string mode, int slot, int station)
        {
            if (!m_PackParser)
            {
                isbformat.Reset(' ');
                isbformat.RunMode = mode.ToCharArray(0, mode.Length);
                isbformat.WorkSlot = slot.ToString().ToArray();
                isbformat.WorkChamber = station.ToString().ToArray();
                writer.Write(isbformat.RunMode);
                writer.Write(isbformat.WorkSlot);
                writer.Write(isbformat.WorkChamber);
            }
            else
            {
                isbformat_parser.Reset(' ');
                isbformat_parser.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(isbformat_parser.RunMode.Length)).ToArray();
                isbformat_parser.WorkSlot = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(isbformat_parser.WorkSlot.Length)).ToArray();
                isbformat_parser.WorkChamber = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(isbformat_parser.WorkChamber.Length)).ToArray();
            }
        }

        private void EJBFormat(string mode, int slot, int station)
        {
            if (!m_PackParser)
            {
                ejbformat.Reset(' ');
                ejbformat.RunMode = mode.ToCharArray(0, mode.Length);
                ejbformat.WorkSlot = slot.ToString().ToArray();
                ejbformat.WorkChamber = station.ToString().ToArray();
                writer.Write(ejbformat.RunMode);
                writer.Write(ejbformat.WorkSlot);
                writer.Write(ejbformat.WorkChamber);
            }
            else
            {
                ejbformat_parser.Reset(' ');
                ejbformat_parser.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ejbformat_parser.RunMode.Length)).ToArray();
                ejbformat_parser.WorkSlot = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ejbformat_parser.WorkSlot.Length)).ToArray();
                ejbformat_parser.WorkChamber = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ejbformat_parser.WorkChamber.Length)).ToArray();
            }
        }

        private void STGFormat(string mode, int slot, int station, string lotno, string type, string program)
        {
            if (!m_PackParser)
            {
                stgformat.Reset(' ');
                stgformat.RunMode = mode.ToCharArray(0, mode.Length);
                stgformat.WorkSlot = slot.ToString().ToArray();
                stgformat.WorkChamber = station.ToString().ToArray();
                stgformat.LotID = lotno.ToCharArray(0, lotno.Length);
                stgformat.BoardType = type.ToCharArray(0, type.Length);
                stgformat.Program = program.ToCharArray(0, program.Length);
                writer.Write(stgformat.RunMode);
                writer.Write(stgformat.WorkSlot);
                writer.Write(stgformat.WorkChamber);
                writer.Write(stgformat.LotID);
                writer.Write(stgformat.BoardType);
                writer.Write(stgformat.Program);
            }
            else
            {
                stgformat_parser.Reset(' ');
                stgformat_parser.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.RunMode.Length)).ToArray();
                stgformat_parser.WorkSlot = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.WorkSlot.Length)).ToArray();
                stgformat_parser.WorkChamber = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.WorkChamber.Length)).ToArray();
                stgformat_parser.LotID = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.LotID.Length)).ToArray();
                stgformat_parser.BoardType = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.BoardType.Length)).ToArray();
                stgformat_parser.Program = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(stgformat_parser.Program.Length)).ToArray();
            }
        }

        private void SDPFormat(string mode, int slot, int station)
        {
            if (!m_PackParser)
            {
                sdpformat.Reset(' ');
                sdpformat.RunMode = mode.ToCharArray(0, mode.Length);
                sdpformat.WorkSlot = slot.ToString().ToArray();
                sdpformat.WorkChamber = station.ToString().ToArray();
                writer.Write(sdpformat.RunMode);
                writer.Write(sdpformat.WorkSlot);
                writer.Write(sdpformat.WorkChamber);
            }
            else
            {
                sdpformat_parser.Reset(' ');
                sdpformat_parser.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(sdpformat_parser.RunMode.Length)).ToArray();
                sdpformat_parser.WorkSlot = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(sdpformat_parser.WorkSlot.Length)).ToArray();
                sdpformat_parser.WorkChamber = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(sdpformat_parser.WorkChamber.Length)).ToArray();
            }
        }
        private void ETSFormat(string mode, int slot, int station)
        {
            if (!m_PackParser)
            {
                etsformat.Reset(' ');
                etsformat.RunMode = mode.ToCharArray(0, mode.Length);
                etsformat.WorkSlot = slot.ToString().ToArray();
                etsformat.WorkChamber = station.ToString().ToArray();
                etsformat.BoardData = m_BibMap.Map;
                writer.Write(etsformat.RunMode);
                writer.Write(etsformat.WorkSlot);
                writer.Write(etsformat.WorkChamber);
                writer.Write(etsformat.BoardData);
            }
            else
            {
                etsformat_parser.Reset(' ');
                etsformat_parser.RunMode = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(etsformat_parser.RunMode.Length)).ToArray();
                etsformat_parser.WorkSlot = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(etsformat_parser.WorkSlot.Length)).ToArray();
                etsformat_parser.WorkChamber = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(etsformat_parser.WorkChamber.Length)).ToArray();
                etsformat_parser.BoardData = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(etsformat_parser.BoardData.Length)).ToArray();
            }
        }

        public byte[] bytedata1 = new byte[1024];
        public byte[] bytedata2 = new byte[1024];
        private void DFMessage(string id, int length = 0)
        {
            try
            {
                if (!m_PackParser)
                {
                    Array.Clear(bytedata1, 0, bytedata1.Length);
                    stream = new MemoryStream();
                    writer = new BinaryWriter(stream);
                    messageformat.TotalLength = length.ToString().ToArray();
                    string tmep = "001";
                    if (tmep.Length > 3)
                    {
                        tmep = tmep.ToString().Substring(tmep.Length - 3, 3);
                    }
                    messageformat.MachingNo = ("M" + tmep).ToArray();
                    messageformat.TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss").ToArray();
                    if (id == "ACKMessage") id = "ACK";
                    if (id == "NCKMessage") id = "NCK";
                    messageformat.MsgID = id.ToCharArray(0, id.Length);
                    writer.Write(messageformat.TotalLength);
                    writer.Write(messageformat.MsgID);
                    writer.Write(messageformat.MachingNo);
                    writer.Write(messageformat.OpID);
                    writer.Write(messageformat.TimeStamp);
                }
                else
                {
                    messageformat_parser.Reset(' ');
                    messageformat_parser.TotalLength = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(messageformat_parser.TotalLength.Length)).ToArray();
                    messageformat_parser.MsgID = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(messageformat_parser.MsgID.Length)).ToArray();
                    messageformat_parser.MachingNo = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(messageformat_parser.MachingNo.Length)).ToArray();
                    messageformat_parser.OpID = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(messageformat_parser.OpID.Length)).ToArray();
                    messageformat_parser.TimeStamp = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(messageformat_parser.TimeStamp.Length)).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CopyArray(char[] source, char[] target)
        {
            int len = Math.Min(source.Length, target.Length);
            Array.Copy(source, 0, target, 0, len);
        }

        public char[] Map
        {
            get
            {
                return m_BibMap.Map;
            }
            set
            {
                char[] _map = value;
                int Length = Math.Min(_map.Length, value.Length);

                for (int i = 0; i < Length; i++)
                {
                    if (i < _map.Length)
                    {
                        m_BibMap.Map[i] = _map[i];
                    }
                }
            }
        }
        public void SetChamberSlot(int slot, int station)
        {
            m_slot = slot;
            m_station = station;
        }

        public void SetMode(string mode)
        {
            m_mode = mode;
        }

        public void SetLotInfo(string lotno, string type, string program)
        {
            m_lotno = lotno;
            m_type = type;
            m_program = program;
        }
        public string GetMessageID()
        {
            return new string(messageformat_parser.MsgID);
        }

        public string GetMessageLength()
        {
            return new string(messageformat_parser.TotalLength);
        }
        public int PackResult(string mode, PackType packtype)
        {
             int Result = 0;
             string ret = new string(messageformat_parser.MsgID).Trim();
             string NckRet = new string(NCKM.ErrorCode).Trim();
             string NckMsg = new string(NCKM.ErrorMessage).Trim();

             try
             {
                switch (packtype)
                {
                    case PackType.ISB:
                        if (ret == "ACK")
                        {
                            Result = 1;
                        }
                        else if (ret == "ISB")
                        {
                            //Return ACK
                            PackFormat(ret, packtype);
                            byte[] Data = bytedata1.Concat(bytedata2).ToArray();
                            Result = 1;
                        }
                        else
                        {
                            if (ret == "NCK")
                            {
                                m_ErrMsg = NckMsg;
                            }
                            Result = -1;    //Insert Error
                        }
                        break;
                    case PackType.EJB:
                        if (ret == "ACK" || ret == "EJB")
                        {
                            Result = 1;
                        }
                        else
                        {
                            if (ret == "NCK")
                            {
                                m_ErrMsg = NckMsg;
                            }
                            Result = -2;    //Eject Error
                        }
                        break;
                    case PackType.STG:
                        if (ret == "ACK" || ret == "STG")
                        {
                            Result = 1;
                        }
                        else
                        {
                            if (ret == "NCK")
                            {
                                m_ErrMsg = NckMsg;
                            }
                            Result = -3;    //Start Test Program Error
                        }
                        break;
                    case PackType.SDP:
                        if (ret == "ACK" || ret == "SDP")
                        {
                            Result = 1;
                        }
                        else
                        {
                            if (ret == "NCK")
                            {
                                m_ErrMsg = NckMsg;
                            }
                            Result = -4;    //Send DePowerError
                        }
                        break;
                    case PackType.ETS:
                        if (ret == "ETS")
                        {
                            m_BibMap.Map = etsformat_parser.BoardData;
                        }
                        else if (ret == "ACK")
                        {
                            Result = 1;
                        }
                        else
                        {
                            if (ret == "NCK")
                            {
                                m_ErrMsg = NckMsg;
                            }
                            Result = -5;    //End Test Program Error
                        }
                        break;
                    case PackType.ACKMessage:
                    case PackType.NCKMessage:
                        Result = 1;
                        break;
                }
             }
             catch (Exception ex)
             {
                string Line = new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString();
                string CurrentName = new StackTrace(true).GetFrame(0).GetMethod().Name;
                string CallName = new StackTrace(true).GetFrame(1).GetMethod().Name;

                JLogger.LogFatal("Exception", "Line:" + Line + "|" + " ModuleName:" + CurrentName + "|" + " FuncName:" + CallName + "|" + ex.Message, false);
                Result = -183;
             }
             return Result;
        }

        public int PackAnalysis(string mode, PackType packtype, byte[] data)
        {
            m_PackParser = true;
            try
            {
                stream = new MemoryStream(data);
                reader = new BinaryReader(stream);
                switch (packtype)
                {
                    case PackType.ACKMessage:
                        ACKFormat(mode);
                        break;
                    case PackType.NCKMessage:
                        NCKFormat(mode);
                        break;
                    case PackType.ISB:
                        ISBFormat(mode, m_slot, m_station);
                        break;
                    case PackType.EJB:
                        EJBFormat(mode, m_slot, m_station);
                        break;
                    case PackType.STG:
                        STGFormat(mode, m_slot, m_station, m_lotno, m_type, m_program);
                        break;
                    case PackType.SDP:
                        SDPFormat(mode, m_slot, m_station);
                        break;
                    case PackType.ETS:
                        ETSFormat(mode, m_slot, m_station);
                        break;
                    case PackType.DFMessage:
                        DFMessage(" ");
                        break;
                }
            }
            catch (Exception ex)
            {
                string Line = new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString();
                string CurrentName = new StackTrace(true).GetFrame(0).GetMethod().Name;
                string CallName = new StackTrace(true).GetFrame(1).GetMethod().Name;

                JLogger.LogFatal("Exception", "Line:" + Line + "|" + " ModuleName:" + CurrentName + "|" + " FuncName:" + CallName + "|" + ex.Message, false);
                return -183;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return 1;
        }
        public int PackFormat(string mode, PackType packtype, string boardid = "")
        {
            m_PackParser = false;
            try
            {
                string id = string.Empty;
                Array.Clear(bytedata1, 0, bytedata1.Length);
                Array.Clear(bytedata2, 0, bytedata2.Length);
                messageformat.Reset(' ');
                stream = new MemoryStream();
                writer = new BinaryWriter(stream);
                switch (packtype)
                {
                    case PackType.ISB:
                        ISBFormat(mode, m_slot, m_station);
                        break;
                    case PackType.EJB:
                        EJBFormat(mode, m_slot, m_station);
                        break;
                    case PackType.STG:
                        STGFormat(mode, m_slot, m_station, m_lotno, m_type, m_program);
                        break;
                    case PackType.SDP:
                        SDPFormat(mode, m_slot, m_station);
                        break;
                    case PackType.ETS:
                        ETSFormat(mode, m_slot, m_station);
                        break;
                    case PackType.ACKMessage:
                        ACKFormat(mode);
                        break;
                    case PackType.NCKMessage:
                        NCKFormat(mode);
                        break;
                }
                bytedata2 = stream.ToArray();
                DFMessage(packtype.ToString(), bytedata2.Length);
                bytedata1 = stream.ToArray();
            }
            catch (Exception ex)
            {
                string Line = new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString();
                string CurrentName = new StackTrace(true).GetFrame(0).GetMethod().Name;
                string CallName = new StackTrace(true).GetFrame(1).GetMethod().Name;

                JLogger.LogFatal("Exception", "Line:" + Line + "|" + " ModuleName:" + CurrentName + "|" + " FuncName:" + CallName + "|" + ex.Message, false);
                return -184;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return 1;
        }
        public string GetErrorMsg()
        {
            return m_ErrMsg;
        }
    }

    public class MCC_BMP
    {
        private int m_serverPort = 0;
        private string m_serverIP = string.Empty;
        private string m_mode = string.Empty;
        private int m_slot = 0;
        private int m_station = 0;
        private string m_map = string.Empty;
        private string m_lotno = string.Empty;
        private string m_type = string.Empty;
        private string m_program = string.Empty;
        private MCC_BibMap m_BibMap = null;
        private SocketConfig m_socketConfig = null;
        private TCPSocket m_Socket = null;
        public MCC_BMP()
        {
            ReadSetup();

            m_BibMap = new MCC_BibMap();
            m_socketConfig = new SocketConfig(m_serverIP, m_serverPort);
            m_Socket = new TCPSocket(m_socketConfig);

        }

        private bool ReadSetup()
        {
            bool bRet = false;
            string _fullPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "System", "Setup.ini");
            if (File.Exists(_fullPath))
            {
                using (TINIFile ini = new TINIFile(_fullPath))
                {
                    m_serverPort = ini.ReadInteger("MCCLink", "ServerPort", 5555);
                    m_serverIP = ini.ReadString("MCCLink", "MainAddress", "127.0.0.1"); //主servic ip
                    bRet = true;
                }
            }
            return bRet;
        }
        private byte[] Action_Pack(PackType type, string boardid = "")
        {
            byte[] Data = null;
            m_BibMap.PackFormat(m_mode, type, boardid);
            Data = m_BibMap.bytedata1.Concat(m_BibMap.bytedata2).ToArray();

            return Data;
        }
        private int Action_GetDataFromServer(byte[] data, PackType type)
        {
            int ret = 0;
            PackType mytype;
            if (m_Socket.Disconnect())
            {
                //JLogger.LogDebug("ServerProtocol", "(Socket Disconnect)", true);
            }
            if (m_Socket.Connect())
            {
                //JLogger.LogDebug("ServerProtocol", "(Socket connect)", true);
                JLogger.LogDebug("ServerProtocol", "(" + System.Text.Encoding.Default.GetString(data) + ")", true);
                ConnectState Result = m_Socket.SendToHost(data, m_BibMap.bytedata2.Length);
                //ConnectState Result = ConnectState.ReceiveSuccessful;
                if (Result.Equals(ConnectState.ReceiveSuccessful))
                {
                    //JLogger.LogDebug("ServerProtocol", "(Result.Equals(ConnectState.ReceiveSuccessful))", true);
                    byte[] byteArr = null;
                    byteArr = System.Text.Encoding.Default.GetBytes(m_Socket.ReceiveData);
                    //byteArr = System.Text.Encoding.Default.GetBytes("EEXT  E    568      568Mf2220231214181540123456789012         953960      QA-272-0                     0                      f22                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                ");
                    Array.Copy(byteArr, 0, m_BibMap.bytedata1, 0, m_BibMap.bytedata1.Length);
                    ret = m_BibMap.PackAnalysis(m_mode, PackType.DFMessage, m_BibMap.bytedata1);
                    
                    //JLogger.LogDebug("ServerProtocol", "(PackAnalysis = " + ret.ToString() + ")", true);
                    if (ret.Equals(1))
                    {
                        string id = m_BibMap.GetMessageID();
                        int MsgLength = int.Parse(m_BibMap.GetMessageLength());
                        switch (MsgLength)
                        {
                            case 20:
                                mytype = PackType.ACKMessage;
                                break;
                            case 280:
                                mytype = PackType.NCKMessage;
                                break;
                            default:
                                mytype = type;
                                break;
                        }
                        
                        m_BibMap.bytedata2 = new byte[MsgLength];
                        Array.Copy(byteArr, m_BibMap.bytedata1.Length, m_BibMap.bytedata2, 0, MsgLength);
                        
                        JLogger.LogDebug("ServerProtocol", "(" + System.Text.Encoding.Default.GetString(byteArr) + ")", true);
                        ret = m_BibMap.PackAnalysis(m_mode, mytype, m_BibMap.bytedata2);
                        
                        //JLogger.LogDebug("ServerProtocol", "(End PackAnalysis = " + ret.ToString() + ")", true);
                    }
                }
                else if (Result.Equals(ConnectState.ConnectTimeOut))//連線TimeOut
                {
                    JLogger.LogDebug("ServerProtocol", "(Result.Equals(ConnectState.ConnectTimeOut)" + ret.ToString() + ")", true);
                    ret = -150;
                }
                else if (Result.Equals(ConnectState.ConnectFail))//STK連線失敗
                {
                    ret = -151;
                }
            }
            else   //STK連線失敗
            {
                ret = -151;
            }

            return ret;

        }
        private string BINTypeTransfer(string chars, bool Mode)//false ToProv ,true ToSTK
        {
            string ret = chars;
            if (m_mode.Equals("Load") || m_mode.Equals("Exchange"))
            {
                if (!Mode)
                {
                    switch (chars)
                    {
                        case "!": ret = "x"; break;
                        case "-": ret = "0"; break;
                        case "0": ret = "1"; break;
                    }
                }
                else
                {
                    switch (chars)
                    {
                        case "1": ret = "0"; break;
                        case "0": ret = "-"; break;
                        case "x": ret = "!"; break;
                    }

                }
            }
            else
            {
                if (!Mode)
                {
                    switch (chars)
                    {
                        case "!": ret = "x"; break;
                        case "-": ret = "0"; break;
                        case "0": ret = "1"; break;
                        case "A": ret = "10"; break;
                        case "B": ret = "11"; break;
                        case "C": ret = "12"; break;
                        case "D": ret = "13"; break;
                        case "E": ret = "14"; break;
                        case "F": ret = "15"; break;
                        case "G": ret = "16"; break;
                        case "H": ret = "17"; break;
                        case "I": ret = "18"; break;
                        case "J": ret = "19"; break;
                        case "K": ret = "20"; break;
                        case "L": ret = "21"; break;
                        case "M": ret = "22"; break;
                        case "N": ret = "23"; break;
                        case "O": ret = "24"; break;
                        case "P": ret = "25"; break;
                        case "Q": ret = "26"; break;
                        case "R": ret = "27"; break;
                        case "S": ret = "28"; break;
                        case "T": ret = "29"; break;
                        case "U": ret = "30"; break;
                        case "V": ret = "31"; break;
                        case "W": ret = "32"; break;
                        case "X": ret = "33"; break;
                        case "Y": ret = "34"; break;
                        case "Z": ret = "35"; break;
                    }

                }
                else
                {
                    switch (chars)
                    {
                        case "x": ret = "!"; break;
                        case "0": ret = "-"; break;
                        case "10": ret = "A"; break;
                        case "11": ret = "B"; break;
                        case "12": ret = "C"; break;
                        case "13": ret = "D"; break;
                        case "14": ret = "E"; break;
                        case "15": ret = "F"; break;
                        case "16": ret = "G"; break;
                        case "17": ret = "H"; break;
                        case "18": ret = "I"; break;
                        case "19": ret = "J"; break;
                        case "20": ret = "K"; break;
                        case "21": ret = "L"; break;
                        case "22": ret = "M"; break;
                        case "23": ret = "N"; break;
                        case "24": ret = "O"; break;
                        case "25": ret = "P"; break;
                        case "26": ret = "Q"; break;
                        case "27": ret = "R"; break;
                        case "28": ret = "S"; break;
                        case "29": ret = "T"; break;
                        case "30": ret = "U"; break;
                        case "31": ret = "V"; break;
                        case "32": ret = "W"; break;
                        case "33": ret = "X"; break;
                        case "34": ret = "Y"; break;
                        case "35": ret = "Z"; break;
                    }

                }
            }

            return ret;

        }
        private string ConvertMap(string _BIBMap, bool Mode) //false ToProv ,true ToSTK
        {

            string tMap = string.Empty;

            if (!Mode)
            {
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (!_BIBMap[j + (16 * i)].Equals(' '))
                        {
                            tMap += BINTypeTransfer(_BIBMap[j + (16 * i)].ToString(), Mode);
                            tMap += ",";
                        }
                    }
                }
            }
            else
            {
                //STK
                int col = 3;
                int row = 2;

                string[] tBIBMap = _BIBMap.Split(',');
                if (tBIBMap.Length < col * row)
                {
                    return "";
                }
                int index = 0;
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (j < col && i < row)
                        {
                            tMap += BINTypeTransfer(tBIBMap[index], Mode);
                            index++;
                        }
                        else
                        {
                            tMap += " ";
                        }

                    }
                }


            }

            return tMap;
        }
        private string ConvertMap2(string _BIBMap, bool Mode) //false ReadFile ,true WriteFile
        {
            if (_BIBMap == "")
            {
                return _BIBMap;
            }
            string tMap = string.Empty;
            string[,] aMap = new string[50, 50];
            int iStart = 0;
            int iEnd = 0;
            int jStart = 0;
            int jEnd = 0;
            int iMax = 0;
            int jMax = 0;
            int MapCol = 3;
            int MapRow = 2;
            //MapCol = 3;
            //  MapRow = 2;
            // Mode = true;
            // _BIBMap = "1,2,3,4,5,6";
            BoardMapDir m_boardMapDir = BoardMapDir.RLTB;
            try
            {
                if (!Mode)
                {
                    string[] tBIBMap = _BIBMap.Split(',');
                    switch (m_boardMapDir)
                    {
                        case BoardMapDir.RLTB:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = 0;
                            jMax = MapCol - 1;
                            break;
                        case BoardMapDir.RLBT:
                            break;
                        case BoardMapDir.LRTB:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = 0;
                            jMax = 0;
                            break;
                        case BoardMapDir.LRBT:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = MapRow - 1;
                            jMax = 0;
                            break;
                        case BoardMapDir.TBLR:
                            iStart = 0;
                            iEnd = MapCol;
                            jStart = 0;
                            jEnd = MapRow;
                            iMax = 0;
                            jMax = 0;
                            break;
                        case BoardMapDir.TBRL:
                            iStart = 0;
                            iEnd = MapCol;
                            jStart = 0;
                            jEnd = MapRow;
                            iMax = MapCol - 1;
                            jMax = 0;
                            break;
                        case BoardMapDir.BTLR:
                            break;
                        case BoardMapDir.BTRL:
                            break;
                    }
                    int ck = 0;
                    for (int i = iStart; i < iEnd; i++)
                    {
                        for (int j = jStart; j < jEnd; j++)
                        {

                            aMap[Math.Abs(iMax - i), Math.Abs(jMax - j)] = tBIBMap[ck++].ToString();

                        }

                    }
                    if (iEnd == MapRow)
                    {
                        for (int i = iStart; i < iEnd; i++)
                        {
                            for (int j = jStart; j < jEnd; j++)
                            {

                                //aMap[Math.Abs(iMax - i), Math.Abs(jMax - j)] = _BIBMap[ck++].ToString();
                                tMap += aMap[i, j];
                                tMap += ",";

                            }

                        }

                    }
                    else
                    {
                        for (int i = iStart; i < jEnd; i++)
                        {
                            for (int j = jStart; j < iEnd; j++)
                            {

                                //aMap[Math.Abs(iMax - i), Math.Abs(jMax - j)] = _BIBMap[ck++].ToString();
                                tMap += aMap[j, i];
                                tMap += ",";

                            }

                        }
                    }



                }
                else
                {
                    string[] tBIBMap = _BIBMap.Split(',');
                    switch (m_boardMapDir)
                    {
                        case BoardMapDir.RLTB:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = 0;
                            jMax = MapCol - 1;
                            break;
                        case BoardMapDir.RLBT:
                            break;
                        case BoardMapDir.LRTB:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = 0;
                            jMax = 0;
                            break;
                        case BoardMapDir.LRBT:
                            iStart = 0;
                            iEnd = MapRow;
                            jStart = 0;
                            jEnd = MapCol;
                            iMax = MapRow - 1;
                            jMax = 0;
                            break;
                        case BoardMapDir.TBLR:
                            iStart = 0;
                            iEnd = MapCol;
                            jStart = 0;
                            jEnd = MapRow;
                            iMax = 0;
                            jMax = 0;
                            break;
                        case BoardMapDir.TBRL:
                            iStart = 0;
                            iEnd = MapCol;
                            jStart = 0;
                            jEnd = MapRow;
                            iMax = MapCol - 1;
                            jMax = 0;
                            break;
                        case BoardMapDir.BTLR:
                            break;
                        case BoardMapDir.BTRL:
                            break;
                    }
                    int ck = 0;
                    for (int i = 0; i < MapRow; i++)
                    {
                        for (int j = 0; j < MapCol; j++)
                        {
                            aMap[i, j] = tBIBMap[ck++];
                        }
                    }

                    ck = 0;
                    if (iEnd == MapRow)
                    {
                        for (int i = iStart; i < iEnd; i++)
                        {
                            for (int j = jStart; j < jEnd; j++)
                            {

                                tMap += aMap[Math.Abs(iMax - i), Math.Abs(jMax - j)];
                                tMap += ",";
                            }

                        }
                    }
                    else
                    {

                        for (int i = iStart; i < iEnd; i++)
                        {
                            for (int j = jStart; j < jEnd; j++)
                            {

                                tMap += aMap[Math.Abs(jMax - j), Math.Abs(iMax - i)];
                                tMap += ",";
                            }

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                string Line = new StackTrace(true).GetFrame(0).GetFileLineNumber().ToString();
                string CurrentName = new StackTrace(true).GetFrame(0).GetMethod().Name;
                string CallName = new StackTrace(true).GetFrame(1).GetMethod().Name;

                JLogger.LogFatal("Exception", "Line:" + Line + "|" + " ModuleName:" + CurrentName + "|" + " FuncName:" + CallName + "|" + ex.Message, false);
            }

            return tMap;
        }
        
        public int Action_ISB(string mode, int slot, int station, ref string _msg)
        {
            int ret = 0;
            m_mode = mode;
            m_slot = slot;
            m_station = station;
            m_BibMap.SetChamberSlot(slot, station);
            byte[] data = Action_Pack(PackType.ISB);
            ret = Convert.ToInt32(Action_GetDataFromServer(data, PackType.ISB));
            if (ret == 1)
            {
                ret = m_BibMap.PackResult(m_mode, PackType.ISB);
            }
            _msg = m_BibMap.GetErrorMsg();
            return ret;
        }

        public int Action_EJB(string mode, int slot, int station, ref string _msg)
        {
            int ret = 0;
            m_mode = mode;
            m_slot = slot;
            m_station = station;
            m_BibMap.SetChamberSlot(slot, station);
            byte[] data = Action_Pack(PackType.EJB);
            ret = Convert.ToInt32(Action_GetDataFromServer(data, PackType.EJB));
            if (ret == 1)
            {
                ret = m_BibMap.PackResult(m_mode, PackType.EJB);
            }
            _msg = m_BibMap.GetErrorMsg();
            return ret;
        }

        public int Action_STG(string mode, int slot, int station, string lotno, string type, string program, ref string _msg)
        {
            int ret = 0;
            m_mode = mode;
            m_slot = slot;
            m_station = station;
            m_lotno = lotno;
            m_type = type;
            m_program = program;
            m_BibMap.SetChamberSlot(slot, station);
            m_BibMap.SetLotInfo(lotno, type, program);
            byte[] data = Action_Pack(PackType.STG);
            ret = Convert.ToInt32(Action_GetDataFromServer(data, PackType.STG));
            if (ret == 1)
            {
                ret = m_BibMap.PackResult(m_mode, PackType.STG);
            }
            _msg = m_BibMap.GetErrorMsg();
            return ret;
        }

        public byte[] Action_SDP(string mode, int slot, int station, ref string _msg)
        {
            int ret = 0;
            m_mode = mode;
            m_slot = slot;
            m_station = station;
            m_BibMap.SetChamberSlot(slot, station);
            byte[] data = Action_Pack(PackType.SDP);
            return data;
            //ret = Convert.ToInt32(Action_GetDataFromServer(data, PackType.SDP));
            //if (ret == 1)
            //{
            //    ret = m_BibMap.PackResult(m_mode, PackType.SDP);
            //}
            //_msg = m_BibMap.GetErrorMsg();
            //return ret;
        }

        public byte[] Action_ETS(string mode, int slot, int station, string smap, ref string _msg)
        {
            int ret = 0;
            m_mode = mode;
            m_slot = slot;
            m_station = station;
            //string[] map_ary = smap.Split(',');
            m_BibMap.Map = smap.Replace(",", "").ToCharArray();

            m_BibMap.SetChamberSlot(slot, station);
            byte[] data = Action_Pack(PackType.ETS);
            return data;
        }

        public string GetMap()
        {
            return ConvertMap2(new string(m_BibMap.Map), false);
        }

        
    }
}
