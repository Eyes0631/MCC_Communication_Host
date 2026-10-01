using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCC_Communication
{
    static class MCCDataStruct
    {
        public class DFMessage
        {
            protected char[] totalLength;
            protected char[] msgID;
            protected char[] machingNo;
            protected char[] opID;
            protected char[] timeStamp;
            public DFMessage()
            {
                totalLength = new char[8];
                msgID = new char[4];
                machingNo = new char[4];
                opID = new char[32];
                timeStamp = new char[14];
            }
            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(TotalLength, value);
                Initial(MsgID, value);
                Initial(MachingNo, value);
                Initial(OpID, value);
                Initial(TimeStamp, value);
            }
            
            public char[] TotalLength
            {
                get
                {
                    return totalLength;
                }
                set
                {
                    int len = Math.Min(value.Length, totalLength.Length);
                    Array.Copy(value, 0, totalLength, totalLength.Length - value.Length, len);
                }
            }

            public char[] MsgID
            {
                get
                {
                    return msgID;
                }
                set
                {
                    int len = Math.Min(value.Length, msgID.Length);
                    Array.Copy(value, 0, msgID, msgID.Length - value.Length, len);
                }
            }

            public char[] MachingNo
            {
                get
                {
                    return machingNo;
                }
                set
                {
                    int len = Math.Min(value.Length, machingNo.Length);
                    Array.Copy(value, 0, machingNo, machingNo.Length - value.Length, len);
                }
            }

            public char[] OpID
            {
                get
                {
                    return opID;
                }
                set
                {
                    int len = Math.Min(value.Length, opID.Length);
                    Array.Copy(value, 0, opID, opID.Length - value.Length, len);
                }
            }

            public char[] TimeStamp
            {
                get
                {
                    return timeStamp;
                }
                set
                {
                    int len = Math.Min(value.Length, timeStamp.Length);
                    Array.Copy(value, 0, timeStamp, timeStamp.Length - value.Length, len);
                }
            }
        }

        public class ACKMessage
        {
            protected char[] runMode;

            public ACKMessage()
            {
                runMode = new char[20];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
            }

            public char[] RunMode
            {
                get
                {
                    return runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, runMode.Length);
                    Array.Copy(value, 0, runMode, runMode.Length - value.Length, len);
                }
            }
        }

        public class NCKMessage
        {
            protected char[] runMode;
            protected char[] errorCode;
            protected char[] errorMessage;

            public NCKMessage()
            {
                runMode = new char[20];
                errorCode = new char[4];
                errorMessage = new char[256];
            }
            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(ErrorCode, value);
                Initial(errorMessage, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] ErrorCode
            {
                get
                {
                    return this.errorCode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.errorCode.Length);
                    Array.Copy(value, 0, this.errorCode, errorCode.Length - value.Length, len);
                }
            }

            public char[] ErrorMessage
            {
                get
                {
                    return this.errorMessage;
                }
                set
                {
                    int len = Math.Min(value.Length, this.errorMessage.Length);
                    Array.Copy(value, 0, this.errorMessage, errorMessage.Length - value.Length, len);
                }
            }
        }

        public class ISB
        {
           private char[] runMode;
           private char[] workSlot;
           private char[] workChamber;

            public ISB()
            {
                runMode = new char[20];
                workSlot = new char[2];
                workChamber = new char[1];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(WorkSlot, value);
                Initial(WorkChamber, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] WorkSlot
            {
                get
                {
                    return this.workSlot;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workSlot.Length);
                    Array.Copy(value, 0, this.workSlot, workSlot.Length - value.Length, len);
                }
            }

            public char[] WorkChamber
            {
                get
                {
                    return this.workChamber;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workChamber.Length);
                    Array.Copy(value, 0, this.workChamber, workChamber.Length - value.Length, len);
                }
            }
        }

        public class EJB
        { 
            private char[] runMode;
            private char[] workSlot;
            private char[] workChamber;

            public EJB()
            {
                runMode = new char[20];
                workSlot = new char[2];
                workChamber = new char[1];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(WorkSlot, value);
                Initial(WorkChamber, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] WorkSlot
            {
                get
                {
                    return this.workSlot;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workSlot.Length);
                    Array.Copy(value, 0, this.workSlot, workSlot.Length - value.Length, len);
                }
            }

            public char[] WorkChamber
            {
                get
                {
                    return this.workChamber;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workChamber.Length);
                    Array.Copy(value, 0, this.workChamber, workChamber.Length - value.Length, len);
                }
            }
        }

        public class SDP
        { 
            private char[] runMode;
            private char[] workSlot;
            private char[] workChamber;

            public SDP()
            {
                runMode = new char[20];
                workSlot = new char[2];
                workChamber = new char[1];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(WorkSlot, value);
                Initial(WorkChamber, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] WorkSlot
            {
                get
                {
                    return this.workSlot;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workSlot.Length);
                    Array.Copy(value, 0, this.workSlot, workSlot.Length - value.Length, len);
                }
            }

            public char[] WorkChamber
            {
                get
                {
                    return this.workChamber;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workChamber.Length);
                    Array.Copy(value, 0, this.workChamber, workChamber.Length - value.Length, len);
                }
            }
        }

        public class STG
        { 
            private char[] runMode;
            private char[] workSlot;
            private char[] workChamber;
            private char[] lotID;
            private char[] boardType;
            private char[] program;

            public STG()
            {
                runMode = new char[20];
                workSlot = new char[2];
                workChamber = new char[1];
                lotID = new char[128];
                boardType = new char[64];
                program = new char[128];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(WorkSlot, value);
                Initial(WorkChamber, value);
                Initial(LotID, value);
                Initial(BoardType, value);
                Initial(Program, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] WorkSlot
            {
                get
                {
                    return this.workSlot;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workSlot.Length);
                    Array.Copy(value, 0, this.workSlot, workSlot.Length - value.Length, len);
                }
            }

            public char[] WorkChamber
            {
                get
                {
                    return this.workChamber;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workChamber.Length);
                    Array.Copy(value, 0, this.workChamber, workChamber.Length - value.Length, len);
                }
            }

            public char[] LotID
            {
                get
                {
                    return this.lotID;
                }
                set
                {
                    int len = Math.Min(value.Length, this.lotID.Length);
                    Array.Copy(value, 0, this.lotID, lotID.Length - value.Length, len);
                }
            }

            public char[] BoardType
            {
                get
                {
                    return this.boardType;
                }
                set
                {
                    int len = Math.Min(value.Length, this.boardType.Length);
                    Array.Copy(value, 0, this.boardType, boardType.Length - value.Length, len);
                }
            }

            public char[] Program
            {
                get
                {
                    return this.program;
                }
                set
                {
                    int len = Math.Min(value.Length, this.program.Length);
                    Array.Copy(value, 0, this.program, program.Length - value.Length, len);
                }
            }
        }

        public class ETS
        { 
            private char[] runMode;
            private char[] workSlot;
            private char[] workChamber;
            private char[] boardData;

            public ETS()
            {
                runMode = new char[20];
                workSlot = new char[2];
                workChamber = new char[1];
                boardData = new char[512];
            }

            private void Initial(char[] arr, char value)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = value;
                }
            }

            public void Reset(char value)
            {
                Initial(RunMode, value);
                Initial(WorkSlot, value);
                Initial(WorkChamber, value);
                Initial(BoardData, value);
            }

            public char[] RunMode
            {
                get
                {
                    return this.runMode;
                }
                set
                {
                    int len = Math.Min(value.Length, this.runMode.Length);
                    Array.Copy(value, 0, this.runMode, runMode.Length - value.Length, len);
                }
            }

            public char[] WorkSlot
            {
                get
                {
                    return this.workSlot;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workSlot.Length);
                    Array.Copy(value, 0, this.workSlot, workSlot.Length - value.Length, len);
                }
            }

            public char[] WorkChamber
            {
                get
                {
                    return this.workChamber;
                }
                set
                {
                    int len = Math.Min(value.Length, this.workChamber.Length);
                    Array.Copy(value, 0, this.workChamber, workChamber.Length - value.Length, len);
                }
            }

            public char[] BoardData
            {
                get
                {
                    return this.boardData;
                }
                set
                {
                    int len = Math.Min(value.Length, this.boardData.Length);
                    Array.Copy(value, 0, this.boardData, boardData.Length - value.Length, len);
                }
            }
        }

    }
}
