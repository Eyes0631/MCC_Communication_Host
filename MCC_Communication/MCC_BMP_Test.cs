using System;

namespace MCC_Communication
{
    class MCC_BMP_Test
    {
        static void Main(string[] args)
        {
            MCC_BMP bmp = new MCC_BMP();
            string errorMsg = string.Empty;

            // 測試 Action_ISB 方法
            int result = bmp.Action_ISB("Load", 1, 1, ref errorMsg);
            Console.WriteLine($"Action_ISB Result: {result}, Error Message: {errorMsg}");

            // 測試 Action_EJB 方法
            result = bmp.Action_EJB("Unload", 1, 1, ref errorMsg);
            Console.WriteLine($"Action_EJB Result: {result}, Error Message: {errorMsg}");

            // 測試 Action_STG 方法
            result = bmp.Action_STG("Start", 1, 1, "Lot123", "TypeA", "ProgramX", ref errorMsg);
            Console.WriteLine($"Action_STG Result: {result}, Error Message: {errorMsg}");

            // 測試 Action_SDP 方法
            result = bmp.Action_SDP("Stop", 1, 1, ref errorMsg);
            Console.WriteLine($"Action_SDP Result: {result}, Error Message: {errorMsg}");

            // 測試 Action_ETS 方法
            result = bmp.Action_ETS("End", 1, 1, ref errorMsg);
            Console.WriteLine($"Action_ETS Result: {result}, Error Message: {errorMsg}");

            // 測試 GetMap 方法
            string map = bmp.GetMap();
            Console.WriteLine($"Map: {map}");
        }
    }
}
